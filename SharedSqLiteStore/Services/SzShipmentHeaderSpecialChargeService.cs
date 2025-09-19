using System;
using System.Collections.Generic;
using System.Linq;

namespace SqLiteDB
{
    public class SzShipmentHeaderSpecialChargeService
    {
        public void Create(SzShipmentHeaderSpecialCharge detail)
        {
            using (var context = new ApplicationDbContext())
            {
                context.SzShipmentHeaderSpecialCharges.Add(detail);
                context.SaveChanges();
            }
        }

        public SzShipmentHeaderSpecialCharge Read(int id)
        {
            using (var context = new ApplicationDbContext())
            {
                return context.SzShipmentHeaderSpecialCharges.Find(id);
            }
        }

        public void Update(SzShipmentHeaderSpecialCharge detail)
        {
            using (var context = new ApplicationDbContext())
            {
                var existingDetail = context.SzShipmentHeaderSpecialCharges.Find(detail.SGMNTID);
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
                var detail = context.SzShipmentHeaderSpecialCharges.Find(id);
                if (detail != null)
                {
                    context.SzShipmentHeaderSpecialCharges.Remove(detail);
                    context.SaveChanges();
                }
            }
        }

        public List<SzShipmentHeaderSpecialCharge> ListAll()
        {
            dynamic rows;
            using (var context = new ApplicationDbContext())
            {
                rows = context.SzShipmentHeaderSpecialCharges.ToList();
                foreach (var detail in rows)
                {
                    Console.WriteLine(detail.ToString());
                }
            }
            return rows;
        }
    }
}