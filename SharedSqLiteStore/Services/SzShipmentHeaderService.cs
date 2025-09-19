using System;
using System.Collections.Generic;
using System.Linq;

namespace SqLiteDB
{
    public class SzShipmentHeaderService
    {
        public void Create(SzShipmentHeader detail)
        {
            using (var context = new ApplicationDbContext())
            {
                context.SzShipmentHeaders.Add(detail);
                context.SaveChanges();
            }
        }

        public SzShipmentHeader Read(int id)
        {
            using (var context = new ApplicationDbContext())
            {
                return context.SzShipmentHeaders.Find(id);
            }
        }

        public void Update(SzShipmentHeader detail)
        {
            using (var context = new ApplicationDbContext())
            {
                var existingDetail = context.SzShipmentHeaders.Find(detail.SGMNTID);
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
                var detail = context.SzShipmentHeaders.Find(id);
                if (detail != null)
                {
                    context.SzShipmentHeaders.Remove(detail);
                    context.SaveChanges();
                }
            }
        }

        public List<SzShipmentHeader> ListAll()
        {
            dynamic rows;
            using (var context = new ApplicationDbContext())
            {
                rows = context.SzShipmentHeaders.ToList();
                foreach (var detail in rows)
                {
                    Console.WriteLine(detail.ToString());
                }
            }
            return rows;
        }
    }
}