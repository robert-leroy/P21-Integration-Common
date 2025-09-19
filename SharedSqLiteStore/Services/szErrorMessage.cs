using System;
using System.Collections.Generic;
using System.Linq;

namespace SqLiteDB
{
    public class SzErrorMessageService
    {
        public void Create(SzErrorMessage detail)
        {
            using (var context = new ApplicationDbContext())
            {
                context.SzErrorMessage.Add(detail);
                context.SaveChanges();
            }
        }

        public SzErrorMessage Read(int id)
        {
            using (var context = new ApplicationDbContext())
            {
                return context.SzErrorMessage.Find(id);
            }
        }

        //public void Update(SzErrorMessage detail)
        //{
        //    using (var context = new ApplicationDbContext())
        //    {
        //        var existingDetail = context.SzErrorMessage.Find(detail.SGMNTID);
        //        if (existingDetail != null)
        //        {
        //            context.Entry(existingDetail).CurrentValues.SetValues(detail);
        //            context.SaveChanges();
        //        }
        //    }
        //}

        public void Delete(int id)
        {
            using (var context = new ApplicationDbContext())
            {
                var detail = context.SzErrorMessage.Find(id);
                if (detail != null)
                {
                    context.SzErrorMessage.Remove(detail);
                    context.SaveChanges();
                }
            }
        }

        public List<SzErrorMessage> ListAll()
        {
            dynamic rows;
            using (var context = new ApplicationDbContext())
            {
                rows = context.SzErrorMessage.ToList();
                foreach (var detail in rows)
                {
                    Console.WriteLine(detail.ToString());
                }
            }
            return rows;
        }
    }
}