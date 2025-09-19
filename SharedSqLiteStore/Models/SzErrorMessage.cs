/*
 * File: SzErrorMessage.cs
 * Project: SharedSqLiteStore
 * Author: Robert LeRoy
 * Created: April 2023
 * Last Updated: September 2025
 * 
 * Description:
 * This class is responsible defining the SzErrorMessage table structure.
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
