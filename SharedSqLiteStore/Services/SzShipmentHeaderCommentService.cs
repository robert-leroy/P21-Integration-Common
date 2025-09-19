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