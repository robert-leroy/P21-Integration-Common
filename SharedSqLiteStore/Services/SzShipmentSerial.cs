using System;
using System.Collections.Generic;
using System.Linq;

namespace SqLiteDB
{
    public class SzShipmentSerialService
    {
        public void Create(SzShipmentSerial detail)
        {
            using (var context = new ApplicationDbContext())
            {
                context.SzShipmentSerials.Add(detail);
                context.SaveChanges();
            }
        }

        public SzShipmentSerial Read(int id)
        {
            using (var context = new ApplicationDbContext())
            {
                return context.SzShipmentSerials.Find(id);
            }
        }

        public void Update(SzShipmentSerial detail)
        {
            using (var context = new ApplicationDbContext())
            {
                var existingDetail = context.SzShipmentSerials.Find(detail.SGMNTID);
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
                var detail = context.SzShipmentSerials.Find(id);
                if (detail != null)
                {
                    context.SzShipmentSerials.Remove(detail);
                    context.SaveChanges();
                }
            }
        }

        public List<SzShipmentSerial> ListAll()
        {
            dynamic rows;
            using (var context = new ApplicationDbContext())
            {
                rows = context.SzShipmentSerials.ToList();
                foreach (var detail in rows)
                {
                    Console.WriteLine(detail.ToString());
                }
            }
            return rows;
        }
    }
}