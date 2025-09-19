/*
 * File: SqListDB.cs
 * Project: SharedSqLiteStore
 * Author: Robert LeRoy
 * Created: April 2023
 * Last Updated: September 2025
 * 
 * Description:
 * This class is responsible for instantiating the SqLiteDB context and creating the database connection.
 * 
 * Dependencies:
 * - SqLiteDB for in-memory database operations
 * 
 * Notes:
 * - Ensure proper configuration of log4net before using this class.
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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace SqLiteDB
{
    public class SqLiteDB
    {
        ApplicationDbContext context = new ApplicationDbContext();
        private void Connect()
        {
            var shipmentDetails = context.SzShipmentDetails.ToList();
            foreach (var detail in shipmentDetails)
            {
                Console.WriteLine(detail.ToString());
            }
        }
    }
}
