using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
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
            // Get export path from config, default to current directory if not specified
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

            // Create export directory if it doesn't exist
            if (!Directory.Exists(exportPath))
            {
                Directory.CreateDirectory(exportPath);
            }
        }

        /// <summary>
        /// Main export method that exports inventory adjustment data to tab-delimited files
        /// </summary>
        public int ExportAllData()
        {
            try
            {
                log.Info($"Export Inventory: Starting data export for {exportDate:yyyy-MM-dd}");

                // Delete existing files for the export date if they exist
                string itemFileName = Path.Combine(exportPath, $"inventoryadjustmentitem_{exportDate:yyyyMMdd}.txt");
                if (File.Exists(itemFileName))
                    File.Delete(itemFileName);

                string serialFileName = Path.Combine(exportPath, $"inventoryadjustmentserial_{exportDate:yyyyMMdd}.txt");
                if (File.Exists(serialFileName))
                    File.Delete(serialFileName);

                // Get all inventory detail records (SZPTRID = "ID05" for inventory adjustments)
                var inventoryDetails = sqlite.SzShipmentDetails
                    .OrderBy(d => d.INVNBR)
                    .ThenBy(d => d.INVDTLSEQ)
                    .ToList();

                log.Info($"Export Inventory: Found {inventoryDetails.Count} inventory detail records");

                int importSet = 0;
                int currentInvoice = 0;

                foreach (var detail in inventoryDetails)
                {
                    importSet++;

                    // Export item record
                    ExportInventoryItem(importSet, detail);

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
        /// Export inventory adjustment item to tab-delimited file
        /// Columns based on P21 Inventory Adjustment Import format
        /// </summary>
        private void ExportInventoryItem(int importSet, SzShipmentDetail detail)
        {
            string fileName = Path.Combine(exportPath, $"inventoryadjustmentitem_{exportDate:yyyyMMdd}.txt");

            try
            {
                // Get the item model/ID from P21
                string itemId = P21Udf.GetItemModel(cnnSQL, detail.ITMMDL, detail.ITMNBR, "", "", "");

                if (string.IsNullOrWhiteSpace(itemId))
                {
                    log.Warn($"Export Inventory: Could not find item ID for ITMMDL: {detail.ITMMDL}, ITMNBR: {detail.ITMNBR}");
                    return;
                }

                // Get header information for additional context
                var header = sqlite.SzShipmentHeaders
                    .FirstOrDefault(h => h.INVNBR == detail.INVNBR);

                // Determine location ID from warehouse
                int locationId = 0;
                if (detail.SZPTRID == "ID04")
                    locationId = GetLocationIdFromWarehouse(detail.WHS);
                else
                    locationId = (header.TERRCD == 22) ? 150 : 350;

                using (StreamWriter writer = new StreamWriter(fileName, true, Encoding.UTF8))
                {
                    // Write data row - columns per P21 import spec
                    writer.WriteLine(string.Join("\t", new string[] {
                        importSet.ToString(),           // Import Set Number
                        itemId,                         // Item ID
                        detail.SHPQTY.ToString("F4"),   // Unit Quantity (positive for increase, negative for decrease)
                        "",                             // Adjustment amount
                        CleanValue(detail.ORDUOM),      // Order Unit of Measure
                        detail.UNTCST.ToString("F4"),   // Unit Cost (optional)
                        ""                              // Not used
                    }));

                    ExportedItemCount++;
                }

                // Export serial numbers for this item
                ExportInventorySerials(importSet, detail);

            }
            catch (Exception ex)
            {
                log.Error($"Export Inventory: Error exporting item - {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Export inventory adjustment serial numbers to tab-delimited file
        /// </summary>
        private void ExportInventorySerials(int importSet, SzShipmentDetail detail)
        {
            string fileName = Path.Combine(exportPath, $"inventoryadjustmentserial_{exportDate:yyyyMMdd}.txt");

            try
            {   
                // Get serial numbers for this detail line
                var serials = sqlite.SzShipmentSerials
                    .Where(s => s.INVNBR == detail.INVNBR &&
                               s.INVSEQ == detail.INVSEQ &&
                               s.INVDTLSEQ == detail.INVDTLSEQ &&
                               s.ITMNBR == detail.ITMNBR)
                    .ToList();

                if (!serials.Any())
                    return;

                // Get the item model/ID from P21
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
                        // Write serial record - columns per P21 import spec
                        writer.WriteLine(string.Join("\t", new string[] {
                            importSet.ToString(),               // Import Set Number
                            itemId,                             // Item ID (must match item record)
                            CleanValue(serial.SRLNBR?.Trim()),  // Serial Number
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

            // Remove tabs, newlines, and special characters that could break tab-delimited format
            return value.Replace("\t", " ")
                       .Replace("\r", " ")
                       .Replace("\n", " ")
                       .Replace("'", "")
                       .Replace("`", "")
                       .Replace("~", "")
                       .Trim();
        }
    }
}