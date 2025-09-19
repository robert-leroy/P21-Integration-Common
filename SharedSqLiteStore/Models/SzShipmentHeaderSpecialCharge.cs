using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FileHelpers;

[DelimitedRecord("|")]
[Table("SzShipmentHeaderSpecialCharge")]
public class SzShipmentHeaderSpecialCharge : SzBase
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
    [FieldNullValue(0)]
    public int INVDTLSEQ { get; set; }
    [Key, Column(Order = 3)]
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
    [FieldNullValue(typeof(DateTime), "1900-01-01")]
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
        return String.Format("Header Special Charge | SZPTRID: {0} - INVNBR: {1} - INVSEQ: {2} - INVDTLSEQ: {3} - SPCCHGSEQ: {4}", SZPTRID, INVNBR, INVSEQ, INVDTLSEQ, SPCCHGSEQ);
    }
}



