using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FileHelpers;

[DelimitedRecord("|")]
[Table("SzErrorMessage")]
public class SzErrorMessage : SzBase
{
    [Key, Column(Order = 0)]
    public string MODULE { get; set; }
    [Key, Column(Order = 1)]
    public DateTime TIMEADD { get; set; }
    [Key, Column(Order = 2)]
    public int INVNBR { get; set; }
    [MaxLength]
    public string SISMMSG { get; set; }
    [MaxLength]
    public string P21MSG { get; set; }

    public void ConvertToUTC()
    {
        TIMEADD = TIMEADD.ToUniversalTime();
    }

    public override string ToString()
    {
        return String.Format($"Module: {MODULE} -  SISM Message: {SISMMSG} P21 Message: {P21MSG} ");
    }
}
