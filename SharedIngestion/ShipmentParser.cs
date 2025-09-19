// Copyright (c) Vero Software, LLC. All rights reserved.

using System;
using System.IO;
using log4net;
using System.Collections.Generic;
using System.Linq;
using FileHelpers;
using log4net.Util;
using System.Configuration;
using SqLiteDB;

namespace IngestSubzeroFiles
{

    public class ShipmentParser
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly DateTime processingDate;
        private readonly ApplicationDbContext sqlite = new ApplicationDbContext();

        /// <summary>
        /// Reads the source files from Azure Storage Container and parses them into Azure Tables   
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        public ShipmentParser(DateTime pd)
        {
            // TODO: replicate this
            //atf.DeleteExistingEntries();
            this.processingDate = pd;
        }

        /// <summary>
        /// Retrieves all files from Subzero FTP Site
        /// </summary>
        /// <param></param>
        /// <returns>Count of Files Received</returns>
        public int ParseLocalShipmentFiles()
        {

            string localPath = ConfigurationManager.AppSettings["ftp-file-local-path"];
            if (!Directory.Exists(localPath))
            {
                Console.WriteLine($"Directory {localPath} does not exist.");
                return 0;
            }

            string[] files = Directory.GetFiles(localPath);
            foreach (string file in files)
            {
                if (file.Contains(processingDate.ToString("yyyyMMdd")))
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
            }

            return 1;

        }

        /// <summary>
        /// Reads the various records from the shipment file.
        /// Add each record to the appropriate Azure Table
        /// 
        /// </summary>
        /// <param></param>
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

            // TODO - Can we handle the errors better?
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
            
            
            //var res = engine.ReadFile(fileName);

            // For each record, add it to the appropriate Azure Table
            foreach (dynamic rec in res)
                try
                {

                    //log.Info($"{rec.ToString()}");

                    //Azure SDK requires it to be UTC.
                    rec.ConvertToUTC();
                    //rec.RowKey = rec.GetRowKey();

                    switch (rec.GetType().Name) 
                    {
                        case "SzShipmentHeader":
                            log.Debug($"Parser: {rec.ToString()}");
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
                            sqlite.SzShipmentSerials.Add(rec);
                            break;
                        case "SzShipmentHeaderSpecialCharge":
                            sqlite.SzShipmentHeaderSpecialCharges.Add(rec);
                            break;
                        case "SzShipmentDetailSpecialCharge":
                            sqlite.SzShipmentDetailSpecialCharges.Add(rec);
                            break;
                        case "SzShipmentDetailComment":
                            sqlite.SzShipmentDetailComments.Add(rec);
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
            sqlite.SaveChanges();
            return 1;

        }

        /// <summary>
        /// Selector to determine whether we have a master or
        /// detail record to import
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
	}
}

