/*
 * File: SzShipmentDetailComments.cs
 * Project: SharedSqLiteStore
 * Author: Robert LeRoy
 * Created: April 2023
 * Last Updated: September 2025
 * 
 * Description:
 * This class is responsible defining the SzShipmentDetailComments table structure.
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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FileHelpers;

[DelimitedRecord("|")]
[Table("SzShipmentDetailComment")]
public class SzShipmentDetailComment : SzBase
{
    public string SGMNTID{ get; set; }
    public string SZPTRID{ get; set; }
    public string SZCUSID{ get; set; }
    public string PTRCUSID{ get; set; }
    [Key, Column(Order = 0)]
    public int INVNBR{ get; set; }
    [Key, Column(Order = 1)]
    [FieldNullValue("")]
    public string INVSEQ{ get; set; }
    [Key, Column(Order = 2)]
    public int? INVDTLSEQ{ get; set; }
    [Key, Column(Order = 3)]
    public int? CMTSEQ{ get; set; }
    public string INTPRT{ get; set; }
    public string CMTTXT{ get; set; }
    public int? ROWNBR{ get; set; }
    public string ROWACT{ get; set; }
    [FieldNullValue(typeof(DateTime), "1900-01-01")]
    [FieldConverter(typeof(SZDateConverter))]
    public DateTime TIMEADD{ get; set; }

    public void ConvertToUTC()
    {
        TIMEADD = TIMEADD.ToUniversalTime();
    }

    public string GetRowKey()
    {
        return INVNBR.ToString() + "_" + INVSEQ.ToString() + "_" + INVDTLSEQ.ToString() + "_" + CMTSEQ.ToString();
    }

    public override string ToString()
    {
        return String.Format("Shipment Detail Comments | SZPTRID: {0} - INVNBR: {1} - INVSEQ: {2} - INVDTLSEQ: {3} - CMTSEQ: {4}", SZPTRID, INVNBR, INVSEQ, INVDTLSEQ, CMTSEQ);
    }
}

