using System;
using System.Collections.Generic;
using System.Linq;

namespace SqLiteDB
{
    public class SzShipmentDetailService
    {
        public void Create(SzShipmentDetail detail)
        {
            using (var context = new ApplicationDbContext())
            {
                context.SzShipmentDetails.Add(detail);
                context.SaveChanges();
            }
        }

        public SzShipmentDetail Read(int id)
        {
            using (var context = new ApplicationDbContext())
            {
                return context.SzShipmentDetails.Find(id);
            }
        }

        public void Update(SzShipmentDetail detail)
        {
            using (var context = new ApplicationDbContext())
            {
                var existingDetail = context.SzShipmentDetails.Find(detail.SGMNTID);
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
                var detail = context.SzShipmentDetails.Find(id);
                if (detail != null)
                {
                    context.SzShipmentDetails.Remove(detail);
                    context.SaveChanges();
                }
            }
        }

        public List<SzShipmentDetail> ListAll()
        {
            dynamic shipmentDetails;
            using (var context = new ApplicationDbContext())
            {
                shipmentDetails = context.SzShipmentDetails.ToList();
                foreach (var detail in shipmentDetails)
                {
                    Console.WriteLine(detail.ToString());
                }
            }
            return shipmentDetails;
        }
    }
}