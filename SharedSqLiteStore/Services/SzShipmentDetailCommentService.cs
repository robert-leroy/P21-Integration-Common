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