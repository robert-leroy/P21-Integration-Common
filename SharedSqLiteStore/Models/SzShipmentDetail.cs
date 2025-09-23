/*
 * File: SzShipmentDetail.cs
 * Project: SharedSqLiteStore
 * Author: Robert LeRoy
 * Created: April 2023
 * Last Updated: September 2025
 * 
 * Description:
 * This class is responsible defining the SzShipmentDetail table structure.
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
[Table("SzShipmentDetail")]
public class SzShipmentDetail : SzBase
{
    public string SGMNTID { get; set; }
    public string SZPTRID { get; set; }
    public string SZCUSID { get; set; }
    public string PTRCUSID { get; set; }
    [Key, Column(Order = 0)]
    [FieldNullValue(0)]
    public int INVNBR { get; set; }
    [Key, Column(Order = 1)]
    [FieldNullValue("")] // Fix: Provide a valid argument for the FieldNullValue attribute  
    public string INVSEQ { get; set; }
    [Key, Column(Order = 2)]
    [FieldNullValue(0)]
    public int INVDTLSEQ { get; set; }
    public int ITMNBR { get; set; }
    public string ITMDSC { get; set; }
    [FieldNullValue(0.0)]
    public double SHPQTY { get; set; }
    [FieldNullValue(0.0)]
    public double RLSQTY { get; set; }
    [FieldNullValue(0.0)]
    public double BOQTY { get; set; }
    public string ORDUOM { get; set; }
    public string CUSTITMNBR { get; set; }
    public string CUSTITMDSC { get; set; }
    [FieldNullValue(0.0)]
    public double BSPRC { get; set; }
    [FieldNullValue(0.0)]
    public double SLGPRC { get; set; }
    [FieldNullValue(0.0)]
    public double CVTSLGPRC { get; set; }
    [FieldNullValue(0.0)]
    public double NETSLSAMT { get; set; }
    public string PRCCD { get; set; }
    public string COMMCD { get; set; }
    public string ITMACTCLS { get; set; }
    public string CRMEMOCD { get; set; }
    [FieldNullValue(0.0)]
    public double SHPWGHT { get; set; }
    [FieldNullValue(0.0)]
    public double EXTWGHT { get; set; }
    [FieldNullValue(0.0)]
    public double UNTWGHT { get; set; }
    public string WGHTUOM { get; set; }
    public string NOCHRGFLG { get; set; }
    [FieldNullValue(0.0)]
    public double NOCHRGVAL { get; set; }
    public string CNTRYCD { get; set; }
    public string HTSNBR { get; set; }
    public string HTSDSC { get; set; }
    [FieldNullValue(0)]
    public int SHPMTNBR { get; set; }
    [FieldConverter(typeof(SZDateConverter))]
    [FieldNullValue(typeof(DateTime), "2025-09-01")]
    public DateTime SHPMTDATE { get; set; }
    [FieldConverter(typeof(SZTimeConverter))]
    public DateTime SHPMTTIME { get; set; }
    public string BOL { get; set; }
    public string WHS { get; set; }
    public string DTLTXSFX { get; set; }
    public string ITMMDL { get; set; }
    public string CRDRSN { get; set; }
    [FieldNullValue(0.0)]
    public double UNTCST { get; set; }
    [FieldNullValue(0)]
    public int? ROWNBR { get; set; }
    public string ROWACT { get; set; }
    [FieldNullValue(typeof(DateTime), "1900-01-01")]
    [FieldConverter(typeof(SZDateConverter))]
    public DateTime TIMEADD { get; set; }

    public void ConvertToUTC()
    {
        SHPMTDATE = SHPMTDATE.ToUniversalTime();
        SHPMTTIME = SHPMTTIME.ToUniversalTime();
        TIMEADD = TIMEADD.ToUniversalTime();
    }
    public string GetRowKey()
    {
        return INVNBR.ToString() + "_" + INVSEQ.ToString() + "_" + INVDTLSEQ.ToString();
    }

    public override string ToString()
    {
        return String.Format("Detail | SZPTRID: {0} - INVNBR: {1} - INVSEQ: {2} - INVDTLSEQ: {3}", SZPTRID, INVNBR, INVSEQ, INVDTLSEQ);
    }
}

