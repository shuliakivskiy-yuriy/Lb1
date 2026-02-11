using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kursova.Services
{
    public class OrderService
    {
        public decimal CalculateDiscount(decimal totalAmount)
        {
            if (totalAmount >= 50000) return 10.0m;
            if (totalAmount >= 10000) return 5.0m;
            return 0.0m;
        }

        public decimal CalculateFinalSum(decimal totalAmount)
        {
            decimal discount = CalculateDiscount(totalAmount);
            return totalAmount - (totalAmount * discount / 100);
        }
    }
}
