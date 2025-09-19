/*
 * File: FileProcessor.cs
 * Project: IngestSubzeroFiles
 * Author: Robert LeRoy
 * Created: April 2023
 * Last Updated: September 2025
 * 
 * Description:
 * This class is responsible for processing files by retrieving them via FTP
 * and parsing them into SqList in-memory tables. It logs the process and handles errors
 * during file ingestion and parsing.
 * 
 * Dependencies:
 * - log4net for logging
 * - FTPController for file retrieval
 * - ShipmentParser for file parsing
 * 
 * Notes:
 * - Returns 999 to indicate FTP failure during parsing.
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;

namespace IngestSubzeroFiles
{
    public class FileProcessor
    {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public FileProcessor()
        {
        }       

        public int IngestFiles(DateTime pd)
        {
            // Get New FTP Files
            var f = new FTPController();
            int fileCount = f.SshShipmentFiles();
            log.Info($"FileProcessor: Completed FTP. {fileCount} file(s) received.");

            // Parse Files into in-memory SqLite DB tables
            var p = new ShipmentParser(pd);
            if (p.ParseLocalShipmentFiles() == 1)
            {
                log.Info("FileProcessor: Completed Shipment Parsing.");
                return fileCount;
            }
            else
            {
                // This is a hack.  If the file count is 0, then the FTP process failed.
                // We return 999 to indicate that the FTP process failed and throw an error.
                log.Error("FileProcessor: Error Parsing Shipment Files.");
                return 999;
            }               

        }

    }
}
