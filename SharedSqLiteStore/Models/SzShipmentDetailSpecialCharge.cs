/*
 * File: SzShipmentDetailSpecialCharges.cs
 * Project: SharedSqLiteStore
 * Author: Robert LeRoy
 * Created: April 2023
 * Last Updated: September 2025
 * 
 * Description:
 * This class is responsible defining the SzShipmentDetailSpecialCharges table structure.
 * Subzero sends these as record type "S4".
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
[Table("SzShipmentDetailSpecialCharge")]
public class SzShipmentDetailSpecialCharge : SzBase
{
    public string SGMNTID { get; set; }
    public string SZPTRID { get; set; }
    public string SZCUSID { get; set; }
    public string PTRCUSID { get; set; }
    [Key, Column(Order = 0)]
    public int INVNBR { get; set; }
    [Key, Column(Order = 1)]
    [FieldNullValue("")]
    public string INVSEQ { get; set; }
    [Key, Column(Order = 2)]
    public int? INVDTLSEQ { get; set; }
    [FieldNullValue(0)]
    public int SPCCHGSEQ { get; set; }
    public string SPCCHGID { get; set; }
    public string SPCCHGCD { get; set; }
    public string SPCCHGDSC { get; set; }
    public string ITMREF { get; set; }
    public double SPCCHGAMT { get; set; }
    public string TAXID { get; set; }
    public int? ROWNBR { get; set; }
    public string ROWACT { get; set; }
    [FieldNullValue(typeof(DateTime), "2000-01-01")]
    [FieldConverter(typeof(SZDateConverter))]
    public DateTime TIMEADD { get; set; }

    public void ConvertToUTC()
    {
        TIMEADD = TIMEADD.ToUniversalTime();
    }

    public string GetRowKey()
    {
        return INVNBR.ToString() + "_" + INVSEQ.ToString() + "_" + INVDTLSEQ.ToString() + "_" + SPCCHGSEQ.ToString();
    }

    public override string ToString()
    {
        return String.Format("Detail Special Charge | SZPTRID: {0} - INVNBR: {1} - INVSEQ: {2} - INVDTLSEQ: {3} - SPCCHGSEQ: {4}", SZPTRID, INVNBR, INVSEQ, INVDTLSEQ, SPCCHGSEQ);
    }
}



