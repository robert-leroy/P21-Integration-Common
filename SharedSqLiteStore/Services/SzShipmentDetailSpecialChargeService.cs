using System;
using System.Collections.Generic;
using System.Linq;

namespace SqLiteDB
{
    public class SzShipmentDetailSpecialChargeService
    {
        public void Create(SzShipmentDetailSpecialCharge detail)
        {
            using (var context = new ApplicationDbContext())
            {
                context.SzShipmentDetailSpecialCharges.Add(detail);
                context.SaveChanges();
            }
        }

        public SzShipmentDetailSpecialCharge Read(int id)
        {
            using (var context = new ApplicationDbContext())
            {
                return context.SzShipmentDetailSpecialCharges.Find(id);
            }
        }

        public void Update(SzShipmentDetailSpecialCharge detail)
        {
            using (var context = new ApplicationDbContext())
            {
                var existingDetail = context.SzShipmentDetailSpecialCharges.Find(detail.SGMNTID);
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
                var detail = context.SzShipmentDetailSpecialCharges.Find(id);
                if (detail != null)
                {
                    context.SzShipmentDetailSpecialCharges.Remove(detail);
                    context.SaveChanges();
                }
            }
        }

        public List<SzShipmentDetailSpecialCharge> ListAll()
        {
            dynamic rows;
            using (var context = new ApplicationDbContext())
            {
                rows = context.SzShipmentDetailSpecialCharges.ToList();
                foreach (var detail in rows)
                {
                    Console.WriteLine(detail.ToString());
                }
            }
            return rows;
        }
    }
}