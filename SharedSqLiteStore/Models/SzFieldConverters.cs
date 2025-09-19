using System;
using FileHelpers;
using FileHelpers.Converters;

public class SZDateConverter : ConverterBase
{
    public override object StringToField(string from)
    {
        if (string.IsNullOrWhiteSpace(from))
        {
            // Return default value if the field is null or empty
            return DateTime.Now.AddDays(30);
        }

        // Attempt to parse the date
        if (DateTime.TryParse(from, out DateTime result))
        {
            return result;
        }

        // Fallback to default value if parsing fails
        return DateTime.Now.AddDays(30);
    }

    public override string FieldToString(object fieldValue)
    {
        if (fieldValue is DateTime dateTime)
        {
            // Format the DateTime as a string for serialization
            return dateTime.ToString("yyyy-MM-dd");
        }

        return string.Empty;
    }
}
public class SZTimeConverter : ConverterBase
{
    public override object StringToField(string from)
    {
        // Convert from string to Time value
        return Convert.ToDateTime(from.Substring(0, 8));
    }

    public override string FieldToString(object fieldValue)
    {
        // format the date as string in year month day format
        return ((DateTime)fieldValue).ToString("HH-mm-dd");
    }

}
