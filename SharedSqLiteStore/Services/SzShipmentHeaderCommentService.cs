/*
 * File: SzShipmentHeaderCommentService.cs
 * Project: SharedSqLiteStore
 * Author: Robert LeRoy
 * Created: April 2023
 * Last Updated: September 2025
 * 
 * Description:
 * This class is responsible handling the database CRUD methods.
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
using System.Collections.Generic;
using System.Linq;

namespace SqLiteDB
{
    public class SzShipmentHeaderCommentService
    {
        public void Create(SzShipmentHeaderComment detail)
        {
            using (var context = new ApplicationDbContext())
            {
                context.SzShipmentHeaderComments.Add(detail);
                context.SaveChanges();
            }
        }

        public SzShipmentHeaderComment Read(int id)
        {
            using (var context = new ApplicationDbContext())
            {
                return context.SzShipmentHeaderComments.Find(id);
            }
        }

        public void Update(SzShipmentHeaderComment detail)
        {
            using (var context = new ApplicationDbContext())
            {
                var existingDetail = context.SzShipmentHeaderComments.Find(detail.SGMNTID);
                if (existingDetail != null)
                {
                    context.Entry(existingDetail).CurrentValues.SetValues(detail);
                    context.SaveChanges();
                }
            }
        }

        public void Delete(int id)
        {
            using (var context = new ApplicationDbContext())
            {
                var detail = context.SzShipmentHeaderComments.Find(id);
                if (detail != null)
                {
                    context.SzShipmentHeaderComments.Remove(detail);
                    context.SaveChanges();
                }
            }
        }

        public List<SzShipmentHeaderComment> ListAll()
        {
            dynamic rows;
            using (var context = new ApplicationDbContext())
            {
                rows = context.SzShipmentHeaderComments.ToList();
                foreach (var detail in rows)
                {
                    Console.WriteLine(detail.ToString());
                }
            }
            return rows;
        }
    }
}