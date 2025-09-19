# Tisdel SubZero Integration

## Nightly job to pull data from SubZero and create P21 Invoices
Each night SubZero posts a file to their FTP server with the previous day's shipment data.  
This data is pulled down and processed to create invoices in P21. 
The invoices are created in P21 and the data is stored in the database.

## General Process
The application runs as a Windows Console application...
1. The new files are downloaded from the FTP server
2. The files are processed and the data is stored in Azure Storage tables (no-sql)
3. The data is then processed in four steps:
   - The AP transactions are processed
   - The Inventory is then updated based on the shipments
   - The AR transactions are processed
   - Current customer balances are created and pushed to the FTP server
   - A summary of the day's transactions is created and emailed to the accounting department

## Running the application
Built into the application is the Epicore Middleware software.  We used both the P21 REST API and .Net Entity API to interface with P21.
There is a configuration file that can be updated to specify connectivity to other systems:
- Log4net configuration - where the logging files are stored
- FTP configuration - how to connect to SubZero
- APIURL and Password - to Connect with P21 API
- Email settings (SMTP and addresses) for sending the daily summary

The application also accepts command line arguments to run specific steps of the process.  For example, if you only want to run the AP transactions, you can pass in the argument `RunAP` and the application will only run that step.  Other options include:
- PostingDate -- Specific the transaction posting date.
- SourceFile= -- Path to a local Source File to Process.  FTP is not used.
- RunAP -- Process AP Only
- RunAR -- Process AR Only
- RunInventory -- Process Inventory Adjustments Only
Multiple options can be combined together.

## System Architecture
The entire application is written in C# and runs as a Windows Console application.  It is scheduled to run nightly using Windows Task Scheduler.  

![Architecture Diagram](https://github.com/Trevarrow-LLC/SubZero-Integration/blob/master/TisdelArchDiagram.png "Architecure")

