using Azure;
using log4net.Core;
using Newtonsoft.Json;
using P21.DomainObject.Entity.Vendor;
using P21.DomainObject.Inventory;
using P21.DomainObject.Logistics;
using P21.DomainObject.Sales.Order;
using P21.DomainObject.Service.Dispatch;
using P21.Transactions.Model.V2;
using P21.UI.Service.Model.Full.V1.Lookup;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Services.Client;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using static System.IdentityModel.Tokens.SecurityTokenHandlerCollectionManager;

namespace P21Integration
{
    public class ExportOrders
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private ApplicationDbContext sqlite = new ApplicationDbContext();
        private SqlConnection cnnSQL;

        private DateTime exportDate;
        private string exportPath;

        public int ExportedOrderCount = 0;
        public int ExportedLineCount = 0;

        public ExportOrders(DateTime pd)
        {
            exportDate = pd;
            // Get export path from config, default to current directory if not specified
            exportPath = ConfigurationManager.AppSettings["export-path"] ?? @".\Exports";

            string connectionString = ConfigurationManager.AppSettings["sql-conn"]; ;
            cnnSQL = new SqlConnection(connectionString);
            try
            {
                cnnSQL.Open();
            }
            catch (Exception ex)
            {
                log.Error($"Integrate AR: Cannot open connection!  Error: {ex.Message}");
            }


            // Create export directory if it doesn't exist
            if (!Directory.Exists(exportPath))
            {
                Directory.CreateDirectory(exportPath);
            }
        }

        /// <summary>
        /// Main export method that exports all SQLite data to tab-delimited files
        /// </summary>
        public int ExportAllData()
        {
            try
            {
                log.Info($"Export: Starting data export for {exportDate:yyyy-MM-dd}");
                int importSet = 0;

                // delete the existing files for the export date if they exist
                string fileName = Path.Combine(exportPath, $"orderheader_{exportDate:yyyyMMdd}.txt");
                if (File.Exists(fileName))
                    File.Delete(fileName);
                fileName = Path.Combine(exportPath, $"orderline_{exportDate:yyyyMMdd}.txt");
                if (File.Exists(fileName))
                    File.Delete(fileName);
                fileName = Path.Combine(exportPath, $"orderheadernotes_{exportDate:yyyyMMdd}.txt");
                if (File.Exists(fileName))  
                    File.Delete(fileName);
                fileName = Path.Combine(exportPath, $"orderheaderwebinfo_{exportDate:yyyyMMdd}.txt");
                if (File.Exists(fileName))
                    File.Delete(fileName);  
                fileName = Path.Combine(exportPath, $"orderlinenotes_{exportDate:yyyyMMdd}.txt");
                if (File.Exists(fileName))
                    File.Delete(fileName);
                fileName = Path.Combine(exportPath, $"ordersalesrep_{exportDate:yyyyMMdd}.txt");
                if (File.Exists(fileName))
                    File.Delete(fileName);  
                fileName = Path.Combine(exportPath, $"orderserial_{exportDate:yyyyMMdd}.txt");
                if (File.Exists(fileName))
                    File.Delete(fileName);

                var headers = sqlite.SzShipmentHeaders                            
                            .ToList();

                foreach (var hdr in headers)
                {

                    importSet++;

                    // Export each entity type
                    ExportHeaders(importSet, hdr.INVNBR);
                    ExportLines(importSet, hdr.INVNBR, hdr.TERRCD);
                    ExportHeaderNotes(importSet, hdr.INVNBR, hdr.INVDT);

                    log.Info($"Export: Completed Invoice {hdr.INVNBR}.");

                }

                log.Info($"Export: Completed successfully. {ExportedOrderCount} orders, {ExportedLineCount} lines exported");
                return ExportedOrderCount;
            }
            catch (Exception ex)
            {
                log.Error($"Export: Error during export - {ex.Message}", ex);
                return 0;
            }
        }

        /// <summary>
        /// Export order headers to tab-delimited file
        /// ALL columns from orderheader.xlsx template
        /// </summary>
        private void ExportHeaders(int importSet, int invoiceNumber)
        {
            string fileName = Path.Combine(exportPath, $"orderheader_{exportDate:yyyyMMdd}.txt");

            try
            {
                var headers = sqlite.SzShipmentHeaders
                    .Where(h => h.INVNBR == invoiceNumber) 
                    .ToList();

                using (StreamWriter writer = new StreamWriter(fileName, true, Encoding.UTF8))
                {
                    // Write data rows (no header row) - ALL columns from Excel template
                    foreach (var hdr in headers)
                    {
                        int locationId = GetLocationId(hdr);

                        writer.WriteLine(string.Join("\t", new string[] {
                            importSet.ToString(),
                            hdr.PTRCUSID,
                            hdr.BTCUSTNM,
                            (hdr.SZPTRID == "ID04") ? "JTD" : "TREV", // company_id
                            locationId.ToString(), // location_id
                            hdr.PONBR, // customer_po_no
                            P21Udf.GetContactID(cnnSQL, hdr.PTRCUSID),
                            hdr.BTCUSTNM, // contact name
                            (hdr.SZPTRID == "ID04") ? "133163_API" : "SOM", // Taker
                            "", // job name
                            hdr.ORDDT.ToString("MM/dd/yyyy"), // order date
                            hdr.INVDT.ToString("MM/dd/yyyy"), // requested date
                            "", // quote
                            "Y", // approved
                            P21Udf.GetCustomerShipTo(cnnSQL, hdr.PTRCUSID).ToString(), // ship_to_id (lookup from customer)
                            hdr.STCUSTNM, // ship_to_name
                            hdr.STADDR1, // ship_to_addr1
                            hdr.STADDR2, // ship_to_addr2
                            hdr.STCITY, // ship_to_city
                            hdr.STSTATE, // ship_to_state
                            hdr.STZIP, // ship_to_zip
                            hdr.STCNTRYCD, // ship_to_country
                            (hdr.SZPTRID == "ID04") ? "10" : (hdr.TERRCD == 22 ? "150" : "350"), // source_location_id (default to IL01 for now - could be determined based on detail lines)
                            P21Udf.GetCarrier(cnnSQL, hdr.CARRID).ToString(), // carrier_id (lookup from sales code)
                            hdr.CARRNM, // carrier_name
                            "", // route
                            "Order Complete", // packing_basis
                            "", // delivery_instructions (blank)
                            P21Udf.GetTerms(cnnSQL, hdr.TRMCD).ToString(), // terms (blank - lookup required)
                            "", // Terms description
                            "", // will call
                            "", // Class 1
                            "", // Class 2
                            "", // Class 3
                            "", // Class 4
                            "", // Class 5
                            "", // rma flag
                            "", // Freight Code
                            "", // Third Party Billing Flag Desc
                            "", // Capture Usage Default
                            "", // Allocate
                            "", // Contract Number
                            "", // Invoice Batch Number
                            "", // Ship To Email Address
                            "", // Set Invoice Exchange Rate Source Desc
                            "", // Ship To Phone
                            "", // Currency ID
                            "", // Apply Builder Allowance Flag
                            "", // Quote Expiration Date
                            "", // Promise Date
                            "", // Import As Quote
                            "", // Quote Number
                            "CO" + hdr.ORDERNBR, // Web Reference Number
                            "Y", // Create Invoice
                            "", // Strategic Pricing Library ID
                            "", // Merchandise Credit
                            "", // Order Type Priority
                            "", // UPS Code
                            "", // Supplier Order No
                            "", // Supplier Release No
                            "", // Placed By Name
                            "", // Req Payment Upon Release
                            "", // Freight Out
                            "", // Ship To Address
                            "", // Quote Type 
                            "", // Homeowner 
                            "", // Installer
                            "", // Building
                            "", // Architect 
                            "", // Designer 
                            "", // Pricing Source
                            "", // Ship to Latitude
                            "", // Ship To Longitude
                            "", // Exemption No
                            "" // Order Number
                        }));

                        ExportedOrderCount++;
                    }
                }

                //log.Info($"Export: Exported {ExportedOrderCount} headers to {fileName}");
            }
            catch (Exception ex)
            {
                log.Error($"Export: Error exporting headers - {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Export order lines to tab-delimited file
        /// ALL columns from orderline.xlsx template
        /// </summary>
        private void ExportLines(int importSet, int invoiceNumber, int territoryCode)
        {
            string fileName = Path.Combine(exportPath, $"orderline_{exportDate:yyyyMMdd}.txt");
            int lineNo = 1;

            try
            {
                var lines = sqlite.SzShipmentDetails
                    .Where(d => d.INVNBR == invoiceNumber)
                    .OrderBy(d => d.INVNBR)
                    .ThenBy(d => d.INVDTLSEQ)
                    .ToList();

                using (StreamWriter writer = new StreamWriter(fileName, true, Encoding.UTF8))
                {
                    int currentInvoice = 0;

                    // Write data rows (no header row) - ALL columns from Excel template
                    foreach (var line in lines)
                    {
                        // Reset line number for each new invoice
                        if (currentInvoice != line.INVNBR)
                        {
                            currentInvoice = line.INVNBR;
                            lineNo = 1;
                        }

                        int locationId;
                        if (line.SZPTRID == "ID04")
                            locationId = GetLocationIdFromWarehouse(line.WHS);
                        else
                            locationId = (territoryCode == 22) ? 150 : 350;

                        double unitPrice = line.SHPQTY > 0 ? line.SLGPRC / line.SHPQTY : 0;
                        string itemId = P21Udf.GetItemModel(cnnSQL, line.ITMMDL, line.ITMNBR, "", "", "");

                        if (itemId == null)
                            continue;

                        writer.WriteLine(string.Join("\t", new[] {
                            importSet.ToString(), // Import Set Number
                            lineNo.ToString(), // Line No
                            itemId.ToString(), // Item ID
                            line.SHPQTY.ToString("F4"), // Unit Quantity
                            CleanValue(line.ORDUOM), // Unit of Measure
                            unitPrice.ToString("F4"), // Unit Price
                            "SKU: " + CleanValue(line.ITMNBR.ToString()), // Extended Description
                            locationId.ToString(), // Source Location ID
                            locationId.ToString(), // Ship Location ID
                            "", // Product Group ID
                            "", // Supplier ID
                            "", // Supplier Name
                            "", // Required Date
                            "", // Expedite Date
                            "", // Will Call
                            "", // Tax Item
                            "", // OK to Interchange
                            "", // Pricing Unit
                            "", // Commission Cost
                            "", // Other Cost
                            "", // PO Cost
                            "", // Disposition
                            "", // Scheduled
                            "Y", // Manual Price Override
                            "", // Commission Cost Edited
                            "", // Other Cost Edited
                            "", // Capture Usage
                            "", // Tag and Hold Class ID
                            "", // Contract Bin ID
                            "", // Contract No.
                            "", // Allocation Qty
                            "", // Promise Date
                            "", // Revision Level
                            "", // Resolve Item Contract
                            "", // Sample
                            "", // Quote Line No.
                            "", // Quote Complete
                            "", // Item Description
                            "", // Invoice No.
                            lineNo.ToString() // Line No
                        }));

                        ExportLineNotes(importSet, line.INVNBR, line.INVSEQ, line.INVDTLSEQ, lineNo);
                        ExportSerials(importSet, line.INVNBR, line.INVSEQ, line.INVDTLSEQ, lineNo);


                        lineNo++;
                        ExportedLineCount++;
                    }
                }

                //log.Info($"Export: Exported {ExportedLineCount} lines to {fileName}");
            }
            catch (Exception ex)
            {
                log.Error($"Export: Error exporting lines - {ex.Message}", ex);
                //throw;
            }

            try
            {
                var spcs = sqlite.SzShipmentDetailSpecialCharges
                    .Where(d => d.INVNBR == invoiceNumber)
                    .OrderBy(d => d.INVNBR)
                    .ThenBy(d => d.INVDTLSEQ)
                    .ToList();

                using (StreamWriter writer = new StreamWriter(fileName, true, Encoding.UTF8))
                {
                    int currentInvoice = 0;

                    // Write data rows (no header row) - ALL columns from Excel template
                    foreach (var spc in spcs)
                    {

                        string itemId = "";

                        if (spc.SZPTRID == "ID04")
                        {
                            switch (spc.SPCCHGCD)
                            {
                                case "800":
                                    itemId = "FREIGHT";
                                    break;
                                case "801":
                                    itemId = "RESTOCKING FEE";
                                    break;
                                case "803":
                                    itemId = "FREIGHT EXCHANGES";
                                    break;
                                case "804":
                                    itemId = "TAXABLE FREIGHT";
                                    break;
                                case "805":
                                    itemId = "FINANCE CHARGE";
                                    break;
                                case "806":
                                    itemId = "LABOR CHARGE";
                                    break;
                                case "807":
                                    itemId = "TAXABLE LABOR/INSTALL";
                                    break;
                                default:
                                    itemId = spc.SPCCHGID;
                                    break;
                            }
                        }
                        else
                        {
                            switch (spc.SPCCHGCD)
                            {
                                case "800":
                                    itemId = "FREIGHT";
                                    break;
                                case "801":
                                    itemId = "RESTOCKING";
                                    break;
                                case "803":
                                    itemId = "CABINET REPLACEMENT";
                                    break;
                                case "804":
                                    itemId = "FREIGHT";
                                    break;
                                case "806":
                                    itemId = "LABOR-WARRANTY";
                                    break;
                                default:
                                    itemId = spc.SPCCHGCD;
                                    break;
                            }
                        }

                            //lineItem.UnitQuantity = 1;
                            //lineItem.UnitOfMeasure = "EA";
                            //lineItem.UnitPrice = spc.SPCCHGAMT;
                            //lineItem.ExtendedDesc = spc.SPCCHGDSC;
                            //lineItem.SourceLocId = orderCreate.LocationId;
                            //lineItem.ShipLocId = orderCreate.LocationId;
                            //lineItem.ProductGroupId = null;
                            //lineItem.ManualPriceOveride = "Y";

                            writer.WriteLine(string.Join("\t", new[] {
                            importSet.ToString(), // Import Set Number
                            lineNo.ToString(), // Line No
                            itemId.ToString(), // Item ID
                            "1", // Unit Quantity
                            "EA", // Unit of Measure
                            spc.SPCCHGAMT.ToString(), // Unit Price
                            spc.SPCCHGDSC, // Extended Description
                            "10", // Source Location ID
                            "10", // Ship Location ID
                            "", // Product Group ID
                            "", // Supplier ID
                            "", // Supplier Name
                            "", // Required Date
                            "", // Expedite Date
                            "", // Will Call
                            "", // Tax Item
                            "", // OK to Interchange
                            "", // Pricing Unit
                            "", // Commission Cost
                            "", // Other Cost
                            "", // PO Cost
                            "", // Disposition
                            "", // Scheduled
                            "Y", // Manual Price Override
                            "", // Commission Cost Edited
                            "", // Other Cost Edited
                            "", // Capture Usage
                            "", // Tag and Hold Class ID
                            "", // Contract Bin ID
                            "", // Contract No.
                            "", // Allocation Qty
                            "", // Promise Date
                            "", // Revision Level
                            "", // Resolve Item Contract
                            "", // Sample
                            "", // Quote Line No.
                            "", // Quote Complete
                            "", // Item Description
                            "", // Invoice No.
                            lineNo.ToString() // Line No
                        }));

                        lineNo++;
                        ExportedLineCount++;
                    }
                }

                //log.Info($"Export: Exported {ExportedLineCount} lines to {fileName}");
            }
            catch (Exception ex)
            {
                log.Error($"Export: Error exporting lines - {ex.Message}", ex);
                throw;
            }

        }

        /// <summary>
        /// Export header notes to tab-delimited file
        /// ALL columns from orderheadernotes.xlsx template
        /// </summary>
        private void ExportHeaderNotes(int importSet, int invoiceNumber, DateTime invoiceDate)
        {
            string fileName = Path.Combine(exportPath, $"orderheadernotes_{exportDate:yyyyMMdd}.txt");

            try
            {
                var notes = sqlite.SzShipmentHeaderComments
                    .Where(n => n.INVNBR == invoiceNumber)
                    .OrderBy(n => n.INVNBR)
                    .ThenBy(n => n.CMTSEQ)
                    .ToList();

                using (StreamWriter writer = new StreamWriter(fileName, true, Encoding.UTF8))
                {
                    // Group by invoice to concatenate comment lines
                    var groupedNotes = notes.GroupBy(n => n.INVNBR);

                    // Write data rows (no header row) - ALL columns from Excel template
                    foreach (var group in groupedNotes)
                    {
                        StringBuilder noteText = new StringBuilder();
                        DateTime? firstDate = null;

                        foreach (var note in group.OrderBy(n => n.CMTSEQ))
                        {
                            noteText.Append(CleanValue(note.CMTTXT) + " ");
                            if (!firstDate.HasValue)
                                firstDate = note.TIMEADD;
                        }

                        writer.WriteLine(string.Join("\t", new[] {
                            importSet.ToString(), // Import Set Number
                            "SOM Import", // topic
                            noteText.ToString().Trim().ToUpper(), // note
                            invoiceDate.ToString("MM/dd/yyyy"), // activation_date
                            "12/31/2049", // Expriation Date
                            invoiceDate.ToString("MM/dd/yyyy"), // entry_date
                            "", // notepad_class_id (blank)
                            "" // mandatory
                        }));
                    }
                }

                //log.Info($"Export: Exported header notes to {fileName}");
            }
            catch (Exception ex)
            {
                log.Error($"Export: Error exporting header notes - {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Export header web info to tab-delimited file
        /// ALL columns from orderheaderwebinfo.xlsx template
        /// </summary>
        private void ExportHeaderWebInfo(int importSet, int invoiceNumber)
        {
            string fileName = Path.Combine(exportPath, $"orderheaderwebinfo_{exportDate:yyyyMMdd}.txt");

            try
            {
                var headers = sqlite.SzShipmentHeaders
                    .Where(h => (h.SALESTYPE == "BLD" || h.SALESTYPE == "DSP" ||
                                 h.SALESTYPE == "MH" || h.SALESTYPE == "PH" || h.SALESTYPE == "PU"))
                    .ToList();

                using (StreamWriter writer = new StreamWriter(fileName, true, Encoding.UTF8))
                {
                    // Write data rows (no header row) - ALL columns from Excel template
                    foreach (var hdr in headers)
                    {
                        // Only export if there's actual web info data
                        if (!string.IsNullOrWhiteSpace(hdr.QUTNM) || !string.IsNullOrWhiteSpace(hdr.QUTCUST))
                        {
                            StringBuilder webInfo = new StringBuilder();
                            
                            if (!string.IsNullOrWhiteSpace(hdr.QUTNM))
                                webInfo.AppendLine(CleanValue(hdr.QUTNM)?.ToUpper());
                            if (!string.IsNullOrWhiteSpace(hdr.QUTCUST))
                                webInfo.AppendLine(CleanValue(hdr.QUTCUST)?.ToUpper());
                            if (!string.IsNullOrWhiteSpace(hdr.QUTADDR1))
                                webInfo.AppendLine(CleanValue(hdr.QUTADDR1)?.ToUpper());
                            
                            // Build city/state/zip line
                            List<string> cityStateZip = new List<string>();
                            if (!string.IsNullOrWhiteSpace(hdr.QUTCITY))
                                cityStateZip.Add(CleanValue(hdr.QUTCITY)?.ToUpper());
                            if (!string.IsNullOrWhiteSpace(hdr.QUTSTATE))
                                cityStateZip.Add(CleanValue(hdr.QUTSTATE)?.ToUpper());
                            if (!string.IsNullOrWhiteSpace(hdr.QUTZIP))
                                cityStateZip.Add(CleanValue(hdr.QUTZIP)?.ToUpper());
                            
                            if (cityStateZip.Count > 0)
                                webInfo.Append(string.Join(", ", cityStateZip));

                            writer.WriteLine(string.Join("\t", new[] {
                                "", // order_no (blank - will be populated from header lookup)
                                "SOM Import", // topic
                                webInfo.ToString().Trim(), // note
                                FormatDate(hdr.INVDT), // activation_date
                                FormatDate(hdr.INVDT), // entry_date
                                "", // notepad_class_id (blank)
                                "N" // mandatory
                            }));
                        }
                    }
                }

                //log.Info($"Export: Exported header web info to {fileName}");
            }
            catch (Exception ex)
            {
                log.Error($"Export: Error exporting header web info - {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Export line notes to tab-delimited file
        /// ALL columns from orderlinenotes.xlsx template
        /// </summary>
        private void ExportLineNotes(int importSet, int invoiceNumber, string invoiceSequence, int invoiceDetailSequence, int lineNumber)
        {
            string fileName = Path.Combine(exportPath, $"orderlinenotes_{exportDate:yyyyMMdd}.txt");

            try
            {
                var notes = sqlite.SzShipmentDetailComments
                    .Where(n => n.INVNBR == invoiceNumber && n.INVSEQ == invoiceSequence && n.INVDTLSEQ == invoiceDetailSequence )
                    .OrderBy(n => n.CMTSEQ)
                    .ToList();

                using (StreamWriter writer = new StreamWriter(fileName, true, Encoding.UTF8))
                {
                    // Group by invoice + line to concatenate comment lines
                    var groupedNotes = notes.GroupBy(n => new { n.INVNBR, n.INVDTLSEQ });

                    // Write data rows (no header row) - ALL columns from Excel template
                    foreach (var group in groupedNotes)
                    {
                        StringBuilder noteText = new StringBuilder();
                        DateTime? firstDate = null;

                        foreach (var note in group.OrderBy(n => n.CMTSEQ))
                        {
                            noteText.Append(CleanValue(note.CMTTXT) + " ");
                            if (!firstDate.HasValue)
                                firstDate = note.TIMEADD;
                        }

                        writer.WriteLine(string.Join("\t", new[] {
                            importSet.ToString(), // Import Set Number
                            lineNumber.ToString(), // line_no (blank - will be calculated)
                            "SOM Import", // topic
                            noteText.ToString().Trim().ToUpper(), // note
                            "", // FormatDate(firstDate), // activation_date
                            "", // FormatDate(firstDate), // entry_date
                            "", // notepad_class_id (blank)
                            "" // mandatory
                        }));
                    }
                }

               // log.Info($"Export: Exported line notes to {fileName}");
            }
            catch (Exception ex)
            {
                log.Error($"Export: Error exporting line notes - {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Export sales rep information to tab-delimited file
        /// ALL columns from ordersalesrep.xlsx template
        /// </summary>
        private void ExportSalesReps(int importSet, int invoiceNumber)
        {
            string fileName = Path.Combine(exportPath, $"ordersalesrep_{exportDate:yyyyMMdd}.txt");

            try
            {
                var headers = sqlite.SzShipmentHeaders.ToList();

                using (StreamWriter writer = new StreamWriter(fileName, true, Encoding.UTF8))
                {
                    // Write data rows (no header row) - ALL columns from Excel template
                    foreach (var hdr in headers)
                    {
                        writer.WriteLine(string.Join("\t", new[] {
                            "", // order_no (blank - will be populated from header lookup)
                            "", // salesrep_id (blank - lookup required)
                            "Y", // primary_salesrep
                            "100" // commission_split
                        }));
                    }
                }

               // log.Info($"Export: Exported sales rep info to {fileName}");
            }
            catch (Exception ex)
            {
                log.Error($"Export: Error exporting sales reps - {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Export serial numbers to tab-delimited file
        /// ALL columns from orderserial.xlsx template
        /// </summary>
        private void ExportSerials(int importSet, int invoiceNumber, string invoiceSequence, int invoiceDetailSequence, int lineNumber)
        {
            string fileName = Path.Combine(exportPath, $"orderserial_{exportDate:yyyyMMdd}.txt");

            try
            {
                var serials = sqlite.SzShipmentSerials
                    .Where(s => s.INVNBR == invoiceNumber && s.INVSEQ == invoiceSequence && s.INVDTLSEQ == invoiceDetailSequence)
                    .OrderBy(s => s.INVNBR)
                    .ThenBy(s => s.INVDTLSEQ)
                    .ToList();

                using (StreamWriter writer = new StreamWriter(fileName, true, Encoding.UTF8))
                {
                    // Write data rows (no header row) - ALL columns from Excel template
                    foreach (var serial in serials)
                    {
                        writer.WriteLine(string.Join("\t", new[] {
                            importSet.ToString(), // Import Set Number  
                            lineNumber.ToString(), // line_no
                            CleanValue(serial.SRLNBR?.Trim()) // serial_number
                        }));
                    }
                }

                //log.Info($"Export: Exported {serials.Count} serials to {fileName}");
            }
            catch (Exception ex)
            {
                log.Error($"Export: Error exporting serials - {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Determine location ID based on warehouse from header's detail lines
        /// </summary>
        private int GetLocationId(SzShipmentHeader hdr)
        {
            var details = sqlite.SzShipmentDetails
                .Where(d => d.INVNBR == hdr.INVNBR)
                .ToList();

            // Check if any items are from IL01 warehouse
            if (details.Any(d => d.WHS == "IL01"))
                return 10;
            else
                return 10224;
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
        /// Check if order should be held (display orders)
        /// </summary>
        private bool IsDisplayOrder(SzShipmentHeader hdr)
        {
            return hdr.PONBR == "TISDEL DISPLAY" || hdr.SALESTYPE?.Contains("RMT") == true;
        }

        /// <summary>
        /// Format date as MM/dd/yy
        /// </summary>
        private string FormatDate(DateTime? date)
        {
            if (!date.HasValue || date.Value.Year < 1900)
                return "";

            return date.Value.ToString("MM/dd/yy");
        }

        /// <summary>
        /// Format date as MM/dd/yy
        /// </summary>
        private string FormatDate(DateTime date)
        {
            if (date.Year < 1900)
                return "";

            return date.ToString("MM/dd/yy");
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