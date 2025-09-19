/*
 * File: SzItemMaster.cs
 * Project: SharedSqLiteStore
 * Author: Robert LeRoy
 * Created: April 2023
 * Last Updated: September 2025
 * 
 * Description:
 * This class is responsible defining the SzItemMaster table structure.
 * 
 * Dependencies:
 * - SqLiteDB for in-memory database operations
 * 
 * Notes:
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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using FileHelpers;

[DelimitedRecord("|")]
[Table("SzItemMaster")]
public class SzItemMaster : SzBase
{
    public string SGMNTID { get; set; }
    public string EMPTY { get; set; }
    [Key, Column(Order = 0)]
    public string ITMNBR{ get; set; }       // Item Number
    public string ITMDSC{ get; set; }       // Item Description
    public string ITMTYP{ get; set; }       // Item Type
    public string INVCD{ get; set; }        // Inventory Flag
    public string ITMCLS{ get; set; }       // Item Class
    public float? ITMACCTCLS{ get; set; }   // Item Accounting Class
    public string CNTYOFORGN { get; set; }  // Country Of Origin
    public string EFFFRMDT { get; set; }    // Effective From Date
    public string EFFTODT { get; set; }     // Effective To Date
    public string STCKUM { get; set; }      // Stocking U/M
    public float? GRSUNTWGT { get; set; }   // Gross Weight
    public string GRSUNTWGTU { get; set; }  // Gross Weight U/M
    public float? NETUNTWGT { get; set; }   // Net Weight
    public string NETUNTWGTU { get; set; }  // Net Weight U/M
    public float? UNTVOL { get; set; }      // Unit Volume
    public string UNTVOLUM { get; set; }    // Unit Volume U/M
    public float? HEIGHT { get; set; }      // Height
    public float? LENGTH { get; set; }      // Length
    public float? WIDTH { get; set; }       // Width
    public float? INDIA { get; set; }       // Inside Diameter
    public float? OUTDIA { get; set; }      // Outside Diameter
    public string DIMUM { get; set; }       // Dimensions U/M
    public string IMPLSTS { get; set; }     // Implementation Status
    public string EXTDSC1 { get; set; }     // Extended Item Description 1
    public string EXTDSC2 { get; set; }     // Extended Item Description 2
    public string VNDRNBR { get; set; }     // Vendor Number
    public string BTCHLT { get; set; }      // Batch/Lot Control Flag
    public string DSCRTALLC { get; set; }   // Discrete Allocation
    public string ITMSLSGRPC { get; set; }  // Item Sales Group Code
    public string ITMSLSGRPD { get; set; }  // Item Sales Group Description
    public string ITMSLSFAMC { get; set; }  // Item Sales Family Code
    public string ITMSLSFAMD { get; set; }  // Item Sales Family Description
    public string BOLCOMMCD { get; set; }   // BoL Commodity Code
    public string PRCEFFFRMDT { get; set; } // Price Effective From Date
    public string PRCEFFTODT { get; set; }  // Price Effective To Date
    public float? BASEPRC{ get; set; }      // Base Price
    public string ITMPRCLS { get; set; }    // Item Price Class
    public string PRCUM { get; set; }       // Price U/M
    public string PRCLN { get; set; }       // Product Line
    public string ITMREG { get; set; }      // Item Region
    public string AVLRSNCD { get; set; }    // Available Reason Code
    public string AVLTYP { get; set; }      // Available Type
    public string AVLFRMDT { get; set; }    // Available From Date
    public string AVLTODT { get; set; }     // Available To Date
    public string AVLCMT { get; set; }      // Available Comment
    public string ITMMDL { get; set; }      // Item Model
    public string ITMMDLGRP { get; set; }   // Item Model Group
    public int? ROWNBR{ get; set; }         // Row Number
    public string ROWACT{ get; set; }       // Row Action
    [FieldNullValue(typeof(DateTime), "1900-01-01")]
    [FieldConverter(typeof(SZDateConverter))]
    public DateTime TIMEADD{ get; set; }    // TimeStamp of when the record was added

    public void ConvertToUTC()
    {
        TIMEADD = TIMEADD.ToUniversalTime();
    }
    public string GetRowKey()
    {
        return ITMNBR.ToString();
    }

    public override string ToString()
    {
        return String.Format($"Master Record | Item Number: {ITMNBR} - Description: {AVLCMT}");
    }
}

