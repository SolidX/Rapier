using System;

namespace Solidus.Rapier.Core
{
    public static class Extensions
    {
        public static int DaysInYear(this DateTime thisDt)
        {
            return DateTime.IsLeapYear(thisDt.Year) ? 366 : 365;
        }
    }
}
