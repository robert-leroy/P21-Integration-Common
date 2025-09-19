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

            // Parse Files into Azure Tables
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
