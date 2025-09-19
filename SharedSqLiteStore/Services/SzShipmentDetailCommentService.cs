/*
 * File: SzShipmentDetailCommentService.cs
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
    public class SzShipmentDetailCommentService
    {
        public void Create(SzShipmentDetailComment detail)
        {
            using (var context = new ApplicationDbContext())
            {
                context.SzShipmentDetailComments.Add(detail);
                context.SaveChanges();
            }
        }

        public SzShipmentDetailComment Read(int id)
        {
            using (var context = new ApplicationDbContext())
            {
                return context.SzShipmentDetailComments.Find(id);
            }
        }

        public void Update(SzShipmentDetailComment detail)
        {
            using (var context = new ApplicationDbContext())
            {
                var existingDetail = context.SzShipmentDetailComments.Find(detail.SGMNTID);
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
                var detail = context.SzShipmentDetailComments.Find(id);
                if (detail != null)
                {
                    context.SzShipmentDetailComments.Remove(detail);
                    context.SaveChanges();
                }
            }
        }

        public List<SzShipmentDetailComment> ListAll()
        {
            dynamic rows;
            using (var context = new ApplicationDbContext())
            {
                rows = context.SzShipmentDetailComments.ToList();
                foreach (var detail in rows)
                {
                    Console.WriteLine(detail.ToString());
                }
            }
            return rows;
        }
    }
}