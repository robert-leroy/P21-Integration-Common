using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace P21Integration
{
    public class ExportInventory
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private ApplicationDbContext sqlite = new ApplicationDbContext();
        private SqlConnection cnnSQL;

        private DateTime exportDate;
        private string exportPath;

        public int ExportedItemCount = 0;
        public int ExportedSerialCount = 0;

        public ExportInventory(DateTime pd)
        {
            exportDate = pd;
            exportPath = ConfigurationManager.AppSettings["export-path"] ?? @".\Exports";

            string connectionString = ConfigurationManager.AppSettings["sql-conn"];
            cnnSQL = new SqlConnection(connectionString);
            try
            {
                cnnSQL.Open();
            }
            catch (Exception ex)
            {
                log.Error($"Export Inventory: Cannot open connection! Error: {ex.Message}");
            }

            if (!Directory.Exists(exportPath))
                Directory.CreateDirectory(exportPath);
        }

        /// <summary>
        /// Main export method that exports inventory adjustment data to tab-delimited files.
        /// Creates four files split by TERRCD: _150 (TERRCD==22) and _350 (TERRCD==150).
        /// </summary>
        public int ExportAllData(string partnerId) 
        {
            try
            {
                log.Info($"Export Inventory: Starting data export for {exportDate:yyyy-MM-dd}");

                string[] sfxList;

                if (partnerId == "ID04")
                {
                    sfxList = new[] { "10", "10224" };
                }
                else
                {  
                    sfxList = new[] { "150", "350" }; 
                }
                
                foreach (string suffix in sfxList)
                    {
                        string itemFile = GetItemFileName(suffix);
                        string serialFile = GetSerialFileName(suffix);

                        if (File.Exists(itemFile)) File.Delete(itemFile);
                        if (File.Exists(serialFile)) File.Delete(serialFile);
                    }

                var inventoryDetails = sqlite.SzShipmentDetails
                    .Where(d => d.SZPTRID == partnerId)
                    .OrderBy(d => d.INVNBR)
                    .ThenBy(d => d.INVDTLSEQ)
                    .ToList();

                log.Info($"Export Inventory: Found {inventoryDetails.Count} inventory detail records");

                int importSet = 0;

                foreach (var detail in inventoryDetails)
                {
                    importSet++;

                    // Get header to determine TERRCD
                    var header = sqlite.SzShipmentHeaders
                        .FirstOrDefault(h => h.INVNBR == detail.INVNBR);

                    if (header == null)
                    {
                        log.Warn($"Export Inventory: No header found for INVNBR {detail.INVNBR}, skipping.");
                        continue;
                    }

                    if (partnerId == "ID05")
                    {
                        // KSS 06/15/2023 Select those items where WHS != 'MID' or (WHS = 'MID' and CRDRSN NOT IN ('', '314')) -- 314 is 'Misc Return'
                        // Usually I'm not a fan of continue statements, but this is a good use case
                        if ((detail.WHS != "MID") || ((detail.WHS == "MID") && ((detail.CRDRSN != "") && (detail.CRDRSN != "314"))))
                        {
                            // Include these rows
                            // Jump to header logic below
                        }
                        else
                        { continue; }
                    }

                    if (partnerId == "ID04")
                    { 
                        if (header.INVTYP == "CM")
                        {
                            log.Info($"SOM INV: Skipping CM {detail.INVNBR} Item {detail.ITMMDL}");
                            continue;
                        }

                        if (detail.WHS != "IL01")
                        {
                            log.Info($"SOM INV: Skipping Invoice {detail.INVNBR} Item {detail.ITMMDL} because warehouse is set to {detail.WHS}");
                            continue;
                        }
                    }

                    // Determine file suffix based on TERRCD
                    string locationSuffix;
                    if (partnerId == "ID04")
                    {
                        // For SZPTRID == ID04, determine location from warehouse
                        if (detail.WHS == "IL01")
                            locationSuffix = "10"; // IL01 maps to 10
                        else
                            locationSuffix = "10224"; // Other warehouses map to 10224
                    }
                    else
                    {
                        locationSuffix = (header.TERRCD == 22) ? "150" : "350";
                    }

                    ExportInventoryItem(importSet, detail, header, locationSuffix);
                }

                log.Info($"Export Inventory: Completed successfully. {ExportedItemCount} items, {ExportedSerialCount} serials exported");
                return ExportedItemCount;
            }
            catch (Exception ex)
            {
                log.Error($"Export Inventory: Error during export - {ex.Message}", ex);
                return 0;
            }
        }

        /// <summary>
        /// Export inventory adjustment item to the appropriate tab-delimited file based on location suffix.
        /// </summary>
        private void ExportInventoryItem(int importSet, SzShipmentDetail detail, SzShipmentHeader header, string locationSuffix)
        {
            string fileName = GetItemFileName(locationSuffix);

            try
            {
                string itemId = P21Udf.GetItemModel(cnnSQL, detail.ITMMDL, detail.ITMNBR, "", "", "");

                if (string.IsNullOrWhiteSpace(itemId))
                {
                    log.Warn($"Export Inventory: Could not find item ID for ITMMDL: {detail.ITMMDL}, ITMNBR: {detail.ITMNBR}");
                    return;
                }

                int locationId = 0;
                if (detail.SZPTRID == "ID04")
                    locationId = GetLocationIdFromWarehouse(detail.WHS);
                else
                    locationId = (header.TERRCD == 22) ? 150 : 350;

                double unitCost = GetItemCost(itemId, header.STSTATE);
                unitCost = Math.Round(unitCost, 3);

                using (StreamWriter writer = new StreamWriter(fileName, true, Encoding.UTF8))
                {
                    writer.WriteLine(string.Join("\t", new string[] {
                        importSet.ToString(),
                        itemId,
                        detail.SHPQTY.ToString("F4"),
                        "",
                        CleanValue(detail.ORDUOM),
                        unitCost.ToString("F4"),
                        ""
                    }));

                    ExportedItemCount++;
                }

                ExportInventorySerials(importSet, detail, locationSuffix);
            }
            catch (Exception ex)
            {
                log.Error($"Export Inventory: Error exporting item - {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Export inventory adjustment serial numbers to the appropriate tab-delimited file based on location suffix.
        /// </summary>
        private void ExportInventorySerials(int importSet, SzShipmentDetail detail, string locationSuffix)
        {
            string fileName = GetSerialFileName(locationSuffix);

            try
            {
                var serials = sqlite.SzShipmentSerials
                    .Where(s => s.INVNBR == detail.INVNBR &&
                               s.INVSEQ == detail.INVSEQ &&
                               s.INVDTLSEQ == detail.INVDTLSEQ &&
                               s.ITMNBR == detail.ITMNBR)
                    .ToList();

                if (!serials.Any())
                    return;

                string itemId = P21Udf.GetItemModel(cnnSQL, detail.ITMMDL, detail.ITMNBR, "", "", "");

                if (string.IsNullOrWhiteSpace(itemId))
                {
                    log.Warn($"Export Inventory: Could not find item ID for serials - ITMMDL: {detail.ITMMDL}, ITMNBR: {detail.ITMNBR}");
                    return;
                }

                using (StreamWriter writer = new StreamWriter(fileName, true, Encoding.UTF8))
                {
                    foreach (var serial in serials)
                    {
                        writer.WriteLine(string.Join("\t", new string[] {
                            importSet.ToString(),
                            itemId,
                            CleanValue(serial.SRLNBR?.Trim()),
                            "",
                            "",
                            "",
                            "",
                            ""
                        }));

                        ExportedSerialCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"Export Inventory: Error exporting serials - {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Returns the item file name for the given location suffix (150 or 350).
        /// </summary>
        private string GetItemFileName(string locationSuffix)
        {
            return Path.Combine(exportPath, $"inventoryadjustmentitem_{exportDate:yyyyMMdd}_{locationSuffix}.txt");
        }

        /// <summary>
        /// Returns the serial file name for the given location suffix (150 or 350).
        /// </summary>
        private string GetSerialFileName(string locationSuffix)
        {
            return Path.Combine(exportPath, $"inventoryadjustmentserial_{exportDate:yyyyMMdd}_{locationSuffix}.txt");
        }

        /// <summary>
        /// Get location ID from warehouse code
        /// </summary>
        private int GetLocationIdFromWarehouse(string warehouse)
        {
            if (warehouse == "IL01")
                return 10;
            else
                return 10224;
        }

        /// <summary>
        /// Clean and sanitize field values for export
        /// </summary>
        private string CleanValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";

            return value.Replace("\t", " ")
                       .Replace("\r", " ")
                       .Replace("\n", " ")
                       .Replace("'", "")
                       .Replace("`", "")
                       .Replace("~", "")
                       .Trim();
        }


        double GetItemCost(string itemModel, string stateCode)
        {

            // Query multiple first item from AP
            // This assumes the cost is the same for all items
            SzShipmentDetail sd = sqlite.SzShipmentDetails
                                        .Where(d => d.ITMMDL == itemModel && d.SZCUSID == "BPID04")
                                        .FirstOrDefault();

            double Cost = 0;

            if (sd != null)
                Cost = sd.SLGPRC / sd.SHPQTY;
            else
                Cost = P21Udf.GetCost(cnnSQL, itemModel);

            return Cost;
        }
    }
}