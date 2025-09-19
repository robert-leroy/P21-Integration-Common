/*
 * File: FtpController.cs
 * Project: IngestSubzeroFiles
 * Author: Robert LeRoy
 * Created: April 2023
 * Last Updated: September 2025
 * 
 * Description:
 * This class is responsible for processing files by retrieving them via FTP or SFTP
 * and saving them into a local path. It logs the process and handles errors
 * during file ingestion and parsing.
 * 
 * Dependencies:
 * - log4net for logging
 * - Renci.SshNet for SFTP operations
 * - FluentFTP for FTP operations
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
using System.Net;
using log4net;
using log4net.Config;
using System.Configuration;
using System.Text;
using Renci.SshNet;
using FluentFTP;
using System.Web.Caching;

namespace IngestSubzeroFiles
{
    public class FTPController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public FTPController()
        {
        }
        /// <summary>
        /// Retrieves all files from Subzero FTP Site
        /// </summary>
        /// <param></param>
        /// <returns>Count of Files Received</returns>
        public int FtpShipmentFiles()
        {

            int fileCount = 0;

            using (var conn = new FtpClient())
            {

                conn.Host = ConfigurationManager.AppSettings["ftp-site"];
                conn.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["ftp-username"], ConfigurationManager.AppSettings["ftp-password"]);
                conn.Connect();

                foreach (var s in conn.GetNameListing(ConfigurationManager.AppSettings["ftp-file-remote-path"]))
                {
                    // load some information about the object
                    // returned from the listing...
                    var isDirectory = conn.DirectoryExists(s);
                    var modify = conn.GetModifiedTime(s);
                    var size = isDirectory ? 0 : conn.GetFileSize(s);

                    // Get last segment of string parsed by /
                    string[] segments = s.Split('/');
                    string filename = segments[segments.Length - 1];

                    if (size > 0)
                    {

                        // string connectionString = ConfigurationManager.AppSettings["azure-conn"];
                        string localPath = ConfigurationManager.AppSettings["ftp-file-local-path"];

                        try
                        {
                            // Read the file from FTP
                            MemoryStream ftpStream = new MemoryStream();
                            conn.DownloadStream(ftpStream, s);
                            ftpStream.Position = 0;


                            // Write file to local path
                            if (localPath != null)
                            {
                                using (FileStream fileStream = File.Create(localPath + filename))
                                {
                                    ftpStream.Position = 0;
                                    ftpStream.CopyTo(fileStream);
                                }
                            }

                            // Remove Processed Files
                            conn.DeleteFile(s); 

                            log.Info($"FTP Get: Received file '{filename}' to source-files container.");
                            fileCount++;

                        }
                        catch (Exception ex) {
                                log.Error($"FTP Get: Error writing file '{s}' to source-files container. {ex.Message}");
                        }
                    }
                }
            }

            return fileCount;

        }


        /// <summary>
        /// Retrieves all files from Subzero FTP Site
        /// </summary>
        /// <param></param>
        /// <returns>Count of Files Received</returns>
        public int SshShipmentFiles()
		{

            int fileCount = 0;
            
            var connectionInfo = new ConnectionInfo(ConfigurationManager.AppSettings["ftp-site"],
                                    ConfigurationManager.AppSettings["ftp-username"],
                                    new PasswordAuthenticationMethod(ConfigurationManager.AppSettings["ftp-username"], ConfigurationManager.AppSettings["ftp-password"]));
            var client = new SftpClient(connectionInfo);
            client.Connect();                

			foreach (var s in client.ListDirectory(ConfigurationManager.AppSettings["ftp-file-remote-path"]))
			{
                // load some information about the object
                // returned from the listing...
                var isDirectory = s.IsDirectory;
                var modify = s.Attributes.LastWriteTime;
				var size = isDirectory ? 0 :s.Attributes.Size;

                if (size > 0)
                {

                    // string connectionString = ConfigurationManager.AppSettings["azure-conn"];
                    string localPath = ConfigurationManager.AppSettings["ftp-file-local-path"];

                    // Get a reference to a container named "sample-container" and then create it
                    //BlobContainerClient container = new BlobContainerClient(connectionString, "source-files");
                    try
                    {
                        // Read the file from FTP
                        MemoryStream ftpStream = new MemoryStream();
                        client.DownloadFile(s.FullName, ftpStream);
                        ftpStream.Position = 0;

                        // Write file to local path
                        if (localPath != null)
                        {
                            using (FileStream fileStream = File.Create(localPath + s.Name))
                            {
                                ftpStream.Position = 0;
                                ftpStream.CopyTo(fileStream);
                            }
                        }

                    }
                    catch(Exception ex)
                    {
                        log.Error($"FTP Get: Error writing file '{s}' to source-files container. {ex.Message}");
                        return 0;
                    }

                    fileCount++;
                    log.Info($"FTP Get: Received file '{s}' to source-files container.");
                    
                    // Delete after processing
                    client.DeleteFile(s.FullName);
                }
            }

            return fileCount;

        }

        /// <summary>
        /// Pushes Balance file to Subzero FTP Site
        /// </summary>
        /// <param>String Builder list of records for pushing to FTP Site.</param>
        /// <returns></returns>
        public void PutBalanceFiles(StringBuilder strCustBal)
        {


            // Format the Date for the new file
            string filename = "SZ_ARCredit_" + DateTime.Now.ToString("yyyyMMdd") + ".TXT";

            // Connect to FTP Site
            var connectionInfo = new ConnectionInfo(ConfigurationManager.AppSettings["ftp-site"],
                                    ConfigurationManager.AppSettings["ftp-username"],
                                    new PasswordAuthenticationMethod(ConfigurationManager.AppSettings["ftp-username"], ConfigurationManager.AppSettings["ftp-password"]));
            var client = new SftpClient(connectionInfo);
            client.Connect();

            // Convert string to stream for upload
            byte[] bytes = Encoding.UTF8.GetBytes(strCustBal.ToString());
            MemoryStream stream = new MemoryStream(bytes);
            client.UploadFile(stream, "/prod/in/" + filename);

            // Shut down the connection
            client.Disconnect();

        }
    }
}

