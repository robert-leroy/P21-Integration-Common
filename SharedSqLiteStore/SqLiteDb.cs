using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace SqLiteDB
{
    public class SqLiteDB
    {
        ApplicationDbContext context = new ApplicationDbContext();
        private void Connect()
        {
            var shipmentDetails = context.SzShipmentDetails.ToList();
            foreach (var detail in shipmentDetails)
            {
                Console.WriteLine(detail.ToString());
            }
        }
    }
}
