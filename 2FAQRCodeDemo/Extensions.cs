using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2FAQRCodeDemo
{
    public static class Extensions
    {
        public static int LimitRange(this int i, int minInclusive, int maxInclusive)
        {
            return Math.Max(Math.Min(i, maxInclusive), minInclusive);
        }
    }
}
