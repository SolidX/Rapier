using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanRepaymentProjector
{
    public static class Extensions
    {
        public static int DaysInYear(this DateTime thisDt)
        {
            return DateTime.IsLeapYear(thisDt.Year) ? 366 : 365;
        }
    }
}
