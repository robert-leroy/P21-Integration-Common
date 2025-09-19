/*
 * File: SzDateConverter.cs
 * Project: SharedSqLiteStore
 * Author: Robert LeRoy
 * Created: April 2023
 * Last Updated: September 2025
 * 
 * Description:
 * This class is responsible defining the SzDateConverter table structure.  To be used during initial import of data.
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
