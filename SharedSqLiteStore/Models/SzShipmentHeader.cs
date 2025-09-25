/*
 * File: SzShipmentHeader.cs
 * Project: SharedSqLiteStore
 * Author: Robert LeRoy
 * Created: April 2023
 * Last Updated: September 2025
 * 
 * Description:
 * This class is responsible defining the SzShipmentHeader table structure.
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
//[Table("SzShipmentHeader")]
public class SzShipmentHeader : SzBase
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
    public string INVTYP { get; set; }
    [FieldConverter(typeof(SZDateConverter))]
    public DateTime INVDT { get; set; }
    [FieldNullValue(typeof(DateTime), "2000-01-01")]
    [FieldConverter(typeof(SZDateConverter))]
    public DateTime NETDUEDT { get; set; }
    public string TRMCD { get; set; }
    public string TRMDSC { get; set; }
    public int TRMNETDYS { get; set; }
    public double TRMPRCNT { get; set; }
    public int TRMDSCNTDY { get; set; }
    public string EXPORT { get; set; }
    public string CURRENCY { get; set; }
    public string VATID { get; set; }
    [FieldNullValue(0)]
    public int SLSREPNBR { get; set; }
    public string ORDERNBR { get; set; }
    public string SALESTYPE { get; set; }
    public string PONBR { get; set; }
    public string SALECD { get; set; }
    public string PRCBKID { get; set; }
    public string CUSTPRCCD { get; set; }
    public string BTCUSTNBR { get; set; }
    public string BTSHPNBR { get; set; }
    public string BTCUSTNM { get; set; }
    public string BTADDR1 { get; set; }
    public string BTADDR2 { get; set; }
    public string BTADDR3 { get; set; }
    public string BTADDR4 { get; set; }
    public string BTADDR5 { get; set; }
    public string BTCITY { get; set; }
    public string BTSTATE { get; set; }
    public string BTZIP { get; set; }
    public string BTCNTRYCD { get; set; }
    public string STCUSTNBR { get; set; }
    public string STSHPNBR { get; set; }
    public string STCUSTNM { get; set; }
    public string STADDR1 { get; set; }
    public string STADDR2 { get; set; }
    public string STADDR3 { get; set; }
    public string STADDR4 { get; set; }
    public string STADDR5 { get; set; }
    public string STCITY { get; set; }
    public string STSTATE { get; set; }
    public string STZIP { get; set; }
    public string STCNTRYCD { get; set; }
    public string SLSORDNBR { get; set; }
    public string PRCHAGT { get; set; }
    public string SLSASSOC { get; set; }
    public string SPECORD { get; set; }
    public string BIDNBR { get; set; }
    public string QUTNM { get; set; }
    public string QUTCUST { get; set; }
    public string QUTADDR1 { get; set; }
    public string QUTADDR2 { get; set; }
    public string QUTCITY { get; set; }
    public string QUTSTATE { get; set; }
    public string QUTZIP { get; set; }
    public string SLCTBLDRRG { get; set; }
    [FieldNullValue(0)]
    public int UNTPRCDSC { get; set; }
    [FieldNullValue(0.0)]
    public double DSCPRCT { get; set; }
    public string INVDSCCD { get; set; }
    [FieldNullValue(0.0)]
    public double INVDSCPRCT { get; set; }
    [FieldNullValue(0.0)]
    public double SHPWGHTOVR { get; set; }
    [FieldNullValue(0)]
    public int SHPMTLDTM { get; set; }
    [FieldNullValue(0.0)]
    public double SHPMTWGHT { get; set; }
    public string DIMUM { get; set; }
    [FieldNullValue(0)]
    public int SHPRREF { get; set; }
    public string SHPINST { get; set; }
    public string CARRID { get; set; }
    public string CARRNM { get; set; }
    public string TRANSMDCD { get; set; }
    public string SHPR { get; set; }
    public string TRLRNBR1 { get; set; }
    public string TRLRNBR2 { get; set; }
    public string TRLRSEAL { get; set; }
    public string TRCKNBR { get; set; }
    public string PROBILLNBR { get; set; }
    public string MSTBOL { get; set; }
    [FieldNullValue(0)]
    public int TERRCD { get; set; }
    public string TERRDESC { get; set; }
    public string HDRTXSFX { get; set; }
    [FieldConverter(typeof(SZDateConverter))]
    [FieldNullValue(typeof(DateTime), "2000-01-01")]
    public DateTime ORDDT { get; set; }
    [FieldNullValue(0.0)]
    public double TOTITMAMT { get; set; }
    [FieldNullValue(0.0)]
    public double TOTSPCCHG1 { get; set; }
    [FieldNullValue(0.0)]
    public double TOTSPCCHG2 { get; set; }
    [FieldNullValue(0.0)]
    public double TOTSPCCHG3 { get; set; }
    [FieldNullValue(0.0)]
    public double TOTSURCHG { get; set; }
    [FieldNullValue(0.0)]
    public double TOTITMTAX { get; set; }
    [FieldNullValue(0.0)]
    public double TOTSPCCHGT { get; set; }
    [FieldNullValue(0.0)]
    public double TOTSURCHGT { get; set; }
    [FieldNullValue(0.0)]
    public double TOTSPCCHGD { get; set; }
    [FieldNullValue(0.0)]
    public double TOTSURCHGD { get; set; }
    [FieldNullValue(0.0)]
    public double TOTTRDDSC { get; set; }
    [FieldNullValue(0.0)]
    public double TOTTRMDSC { get; set; }
    [FieldNullValue(0.0)]
    public double TOTINVAMT { get; set; }
    public int? ROWNBR { get; set; }
    public string ROWACT { get; set; }
    [FieldNullValue(typeof(DateTime), "2000-01-01")]
    [FieldConverter(typeof(SZDateConverter))]
    public DateTime TIMEADD { get; set; }

    public void ConvertToUTC()
    {
        TIMEADD = TIMEADD.ToUniversalTime();
        ORDDT = ORDDT.ToUniversalTime();
        INVDT = INVDT.ToUniversalTime();
        NETDUEDT = NETDUEDT.ToUniversalTime();
    }

    public string GetRowKey()
    {
        return INVNBR.ToString() + "_" + INVSEQ.ToString();
    }

    public override string ToString()
    {
        return String.Format("Header | SZPTRID: {0} - INVNBR: {1}", SZPTRID, INVNBR);
    }
}
