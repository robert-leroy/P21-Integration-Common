using System;
using FileHelpers;

public class SzBase 
{

    [FieldHidden]
    public string PartitionKey { get; set; }
    [FieldHidden]
    public string RowKey { get; set; }
    [FieldHidden]
    public DateTimeOffset? Timestamp { get; set; }

}
