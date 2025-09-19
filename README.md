# Common Integration Components

These are common files used during data integration between two companies.

## Use Case
The aren't much use except in the context of a specific integration.  However; they might be useful for educational purposes.

## Components
This repository contains the following components:
- **Data Models**: Standardized data models for data format.  These are defined for use with SqLite in-memory databases.
- **File Parsing**: A set of files to retrieve the data from FTP servers, parse the data by record type and load it into the SqLite database.

These are share projects.  If you want to use them, you can include them in your own solution as project references. Add the Shared Project to Other Projects:
- Right-click on the projects that need to use the shared code and select Add > Reference.
- In the Reference Manager, go to the Shared Projects section and select the shared project.

The based projects would need to call these are necessary.

