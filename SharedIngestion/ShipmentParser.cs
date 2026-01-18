/*
 * File: ShipmentParser.cs
 * Project: IngestSubzeroFiles
 * Author: Robert LeRoy
 * Created: April 2023
 * Last Updated: September 2025
 * 
 * Description:
 * This class is responsible for spliting the shipment files into their individual record types
 * and adding them to the appropriate SqList database tables.
 * 
 * Dependencies:
 * - log4net for logging
 * - FileHelpers for parsing fixed-width files
 * - SqLiteDB for in-memory database operations
 * 
 * Notes:
 * - Ensure proper configuration of log4net before using this class.
 * 
 * Copyright 2025 Robert LeRoy, Vero Software, LLC.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.IO;
using log4net;
using System.Collections.Generic;
using System.Linq;
using FileHelpers;
using log4net.Util;
using System.Configuration;
using SqLiteDB;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;

namespace IngestSubzeroFiles
{

    public class ShipmentParser
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly DateTime processingDate;
        private readonly ApplicationDbContext sqlite = new ApplicationDbContext();

        /// <summary>
        /// Constructor for ShipmentParser class.  It initializes the processing date.
        /// </summary>
        /// <param name="pd">Processing date to filter files.</param>
        public ShipmentParser(DateTime pd)
        {
            // TODO: replicate this
            //atf.DeleteExistingEntries();
            this.processingDate = pd;
        }

        /// <summary>
        /// Loops through all files in the local path and calls the ParseFile method for each file
        /// </summary>
        /// <param></param>
        /// <returns>1 if successful, 0 if failure</returns>
        public int ParseLocalShipmentFiles()
        {

            string tempPath = ConfigurationManager.AppSettings["ftp-file-temp-path"];
            string localPath = ConfigurationManager.AppSettings["ftp-file-local-path"];
            if (!Directory.Exists(tempPath))
            {
                Console.WriteLine($"Temp Directory {tempPath} does not exist.");
                return 0;
            }
            if (!Directory.Exists(localPath))
            {
                Console.WriteLine($"Local Directory {localPath} does not exist.");
                return 0;
            }

            string[] files = Directory.GetFiles(tempPath);
            foreach (string file in files)
            {
                // only process files that contain the processing date in the filename
                // or the last write date is the processing date
                // and the name is SZ_Shipment or SZ_ItemMstr
                if ((file.Contains(processingDate.ToString("yyyyMMdd")) || File.GetLastWriteTime(file).Date == processingDate.Date)
                    && (file.Contains("SZ_Shipment_") || file.Contains("SZ_ItemMstr")))
                {
                    try
                    {
                        // Process each file
                        log.Debug($"Parser: Processing file {file}.");
                        if (ParseFile(file) == 0)
                            return 0;

                    }
                    catch (Exception ex)
                    {
                        log.Info($"Error processing file {file}: {ex.Message}.");
                    }
                }
                // Move file from temp-path to local-path
                string destinationFile = Path.Combine(localPath, Path.GetFileName(file));
                try
                {
                    if (File.Exists(destinationFile))
                    {
                        File.Delete(destinationFile); // Delete the existing file
                    }
                    File.Move(file, destinationFile);
                    log.Debug($"Parser: Moved file {file} to {destinationFile}.");
                }
                catch (Exception ex)
                {
                    log.Error($"Parser: Failed to move file {file} to {destinationFile}. Error: {ex.Message}");
                }
            }

            return 1;

        }

        /// <summary>
        /// Reads the various records from the shipment file.
        /// Add each record to the appropriate SqlList Table
        /// </summary>
        /// <param name="fileName">The filename of the local file to process</param>
        /// <returns>Count of Files Received</returns>
        public int ParseFile(string fileName) 
        {

            // Create a parser instance for each record type
            var engine = new MultiRecordEngine(
                typeof(SzShipmentHeader),
                typeof(SzShipmentDetail),
                typeof(SzShipmentHeaderComment),
                typeof(SzShipmentSerial),
                typeof(SzShipmentHeaderSpecialCharge),
                typeof(SzShipmentDetailSpecialCharge),
                typeof(SzShipmentDetailComment),
                typeof(SzItemMaster));

            // Set the selector to the Subzero Function
            engine.RecordSelector = new RecordTypeSelector(SubzeroSelector);

            // Read the file
            Object[] res = null;
            try
            {
                res = engine.ReadFile(fileName);
            }
            catch (FileHelpers.ConvertException e)
            {
                log.Error($"Parser:  Error - {e.Message} on line {e.LineNumber}, field name {e.FieldName}, Column Number {e.ColumnNumber}");
                return 0;
            }
            catch (FileHelpers.BadUsageException e)
            {
                log.Error($"Parser:  Error - {e.Message}");
                return 0;
            }            
                       
            // For each record, add it to the appropriate Azure Table
            foreach (dynamic rec in res)
                try
                {

                    rec.ConvertToUTC();

                    switch (rec.GetType().Name) 
                    {
                        case "SzShipmentHeader":
                            log.Info($"Parser: {rec.ToString()}");
                            sqlite.SzShipmentHeaders.Add(rec);
                            break;
                        case "SzShipmentDetail":
                            sqlite.SzShipmentDetails.Add(rec);
                            break;
                        case "SzShipmentHeaderComment":
                            sqlite.SzShipmentHeaderComments.Add(rec);
                            break;
                        case "SzShipmentSerial":
                            SzShipmentSerial s = (SzShipmentSerial)rec;
                            s.SRLNBR.Trim();
                            sqlite.SzShipmentSerials.Add(s);
                            break;
                        case "SzShipmentHeaderSpecialCharge":
                            sqlite.SzShipmentHeaderSpecialCharges.Add(rec);
                            break;
                        case "SzShipmentDetailSpecialCharge":
                            sqlite.SzShipmentDetailSpecialCharges.Add(rec);
                            break;
                        case "SzShipmentDetailComment":
                            // RJL 10/01/2025  
                            // The vendor doesn't send CMTSEQ, so we will generate one here 
                            // I'm using Miliseconds, which should be unique enough for our purposes
                            SzShipmentDetailComment c = (SzShipmentDetailComment)rec;
                            c.CMTSEQ = DateTime.Now.Millisecond;
                            sqlite.SzShipmentDetailComments.Add(c);
                            break;
                        case "SzItemMaster":
                            sqlite.SzItemMasters.Add(rec);
                            break;
                    }
                }
                catch (Exception e)
                {
                    log.Info($"Parsesr:  Error - {e.Message}");
                    return 0;
                }
            try
            {
                sqlite.SaveChanges();
            }
            catch (DbEntityValidationException dbe)
            {
                foreach (DbEntityValidationResult ve in dbe.EntityValidationErrors)
                    foreach (DbValidationError err in ve.ValidationErrors)
                        log.Error($"Parser:  Property: {ve.Entry.Entity} Error: {err.ErrorMessage}");

                log.Error($"Parser:  Error saving shipments to database.  Processing aborted.");
                return 0;
            }
            catch (DbUpdateException dbe)
            {
                log.Error($"Parser: {dbe.InnerException.InnerException.Message}");
                log.Error($"Parser:  Error saving shipments to database.  Processing aborted.");
                return 0;
            }

            return 1;

        }

        /// <summary>
        /// Selector to determine type of record to import
        /// </summary>
        /// <param name="record">Alpha characters coming in</param>
        /// <returns>Selector for master or detail record</returns>
        private Type SubzeroSelector(MultiRecordEngine engine, string record)
        {

            // Skip blank lines
            if (record.Length == 0)
                return null;

            // Skip Header Rows
            if (record.Substring(0,6) == "Header")
                return null;

            // Check the first two characters of record to determine record type
            switch (record.Substring(0, 2))
            {
                case "SH":
                    return typeof(SzShipmentHeader);
                case "SD":
                    return typeof(SzShipmentDetail);
                case "SC":
                    return typeof(SzShipmentHeaderComment);
                case "S2":
                    return typeof(SzShipmentSerial);
                case "S3":
                    return typeof(SzShipmentDetailComment);
                case "S4":
                    return typeof(SzShipmentDetailSpecialCharge);
                // RJL - 09/12/2025 - Ignore these SS records per Ashley
                //case "SS":
                //    return null;
                case "IM":
                    return typeof(SzItemMaster);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Confirm each header has detail lines.  If not,we delete the header record, log the error, and continue
        /// </summary>
        /// <param name="subzeroPartnerID">The Subzero Partner ID to check</param>
        public int ValidateDetailLines(string subzeroPartnerID)
        {
            int returnCode = 1;

            // Get the headers
            IList<SzShipmentHeader> headers = sqlite.SzShipmentHeaders
                                                    .Where(h => h.SZPTRID == subzeroPartnerID && h.INVTYP == "IV")
                                                    .ToList();

            foreach (SzShipmentHeader header in headers)
            {
                // Get the headers
                IList<SzShipmentDetail> details = sqlite.SzShipmentDetails
                                                        .Where(d => d.SZPTRID == subzeroPartnerID && d.INVNBR == header.INVNBR)
                                                        .ToList();

                if (details.Count == 0)
                {
                    log.Error($"Parser:  Shipment Header INVNBR {header.INVNBR} for Customer {header.PTRCUSID} with PO {header.PONBR} has no associated detail records.");

                    // Delete the header record
                    sqlite.SzShipmentHeaders.Remove(header);
                    sqlite.SaveChanges();

                    // Log the error and continue
                    sqlite.SzErrorMessage.Add(new SzErrorMessage()
                    {
                        MODULE = "Parser",
                        INVNBR = header.INVNBR,
                        SISMMSG = $"Shipment Header INVNBR {header.INVNBR} for Customer {header.PTRCUSID} with PO {header.PONBR} has no associated detail records.",
                        TIMEADD = DateTime.Now
                    });
                    sqlite.SaveChanges();

                    returnCode = 0;

                }
            }

            return returnCode;
        }
    }
}

