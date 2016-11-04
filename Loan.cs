using System;
using System.Collections.Generic;

namespace LoanRepaymentProjector
{
    public class Loan
    {
        private static int _idGenerator = 0;

        public readonly int Id;

        /// <summary>
        /// Loan's interest rate
        /// </summary>
        public decimal InterestRate { get; set; }

        /// <summary>
        /// The principal balance of the loan
        /// </summary>
        public decimal Principal { get; private set; }

        /// <summary>
        /// The Date for which the Loan's principal balance applies.
        /// </summary>
        public DateTime PrincipalEffectiveDate { get; private set; }

        /// <summary>
        /// The minimum payment amount allowed for this loan.
        /// </summary>
        public decimal MinimumPayment { get; set; }

        /// <summary>
        /// A human readable name for this loan
        /// </summary>
        public string LoanName { get; set; }

        public Loan()
        {
            Id = _idGenerator;
            _idGenerator++;
        }

        private Loan(int id, decimal principal, DateTime principalDt)
        {
            Id = id;
            Principal = principal;
            PrincipalEffectiveDate = principalDt;
        }

        /// <summary>
        /// Sets the <see cref="Principal"/> balance of the loan and its associated <see cref="PrincipalEffectiveDate"/>
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="asOf"></param>
        public void SetPrincipal(decimal amount, DateTime asOf)
        {
            Principal = amount;
            PrincipalEffectiveDate = asOf;
        }

        /// <summary>
        /// Calcuates the amount of interest accruing daily on the loan based on the number of days in the year. 
        /// </summary>
        /// <returns></returns>
        public decimal CurrentDailyInterestRate()
        {
            return (Principal * InterestRate) / (decimal) PrincipalEffectiveDate.DaysInYear();
        }

        /// <summary>
        /// The interest accrued on the loan (calculated based on the number of days from the <see cref="PrincipalEffectiveDate"/>)
        /// </summary>
        /// <param name="asOf">The date to calculate accrued interest for</param>
        /// <returns>The interest accrued on the loan as of the provided date.</returns>
        /// <exception cref="InvalidOperationException">When the provided <paramref name="asOf"/> date is before the loan's <see cref="PrincipalEffectiveDate"/>.</exception>
        public decimal AccruedInterest(DateTime asOf)
        {
            if (asOf < PrincipalEffectiveDate) throw new InvalidOperationException();

            var total = 0.00m;

            for (var i = PrincipalEffectiveDate.Year; i <= asOf.Year; i++)
            {
                if (i == PrincipalEffectiveDate.Year)
                {
                    var dateDiff = (new DateTime(PrincipalEffectiveDate.Year, 12, 31) - PrincipalEffectiveDate).Days;
                    total += ((Principal * InterestRate) / (decimal)PrincipalEffectiveDate.DaysInYear()) * dateDiff;
                    continue;
                }
                if (i == asOf.Year)
                {
                    var dateDiff = (asOf - new DateTime(asOf.Year, 1, 1)).Days;
                    total += ((Principal * InterestRate) / (decimal)asOf.DaysInYear()) * dateDiff;
                    continue;
                }

                var daysInYear = new DateTime(i, 1, 1).DaysInYear();
                total += ((Principal * InterestRate) / (decimal)daysInYear) * (decimal)daysInYear;
            }

            return total;
        }

        /// <summary>
        /// Calcuates the interest accrued between DateTime.Now and the <see cref="PrincipalEffectiveDate"/>.
        /// </summary>
        /// <returns>The interest accrued on the loan.</returns>
        /// <exception cref="InvalidOperationException">When the loan's <see cref="PrincipalEffectiveDate"/> is in the future.</exception>
        public decimal AccruedInterestToDate()
        {
            return AccruedInterest(DateTime.Now);
        }

        /// <summary>
        /// The sum of the interest accrued up until <paramref name="asOf"/> and the <see cref="Principal"/> balance.
        /// </summary>
        /// <param name="asOf">The date to calculate accrued interest for</param>
        /// <returns>The total balance owed on the loan as of the provided date.</returns>
        /// <exception cref="InvalidOperationException">When the provided <paramref name="asOf"/> date is before the loan's <see cref="PrincipalEffectiveDate"/>.</exception>
        public decimal TotalOwed(DateTime asOf)
        {
            return AccruedInterest(asOf) + Principal;
        }

        /// <summary>
        /// The sum of <see cref="AccruedInterestToDate"/> and <see cref="Principal"/>
        /// </summary>
        /// <returns>The total balance currently owed on the loan.</returns>
        /// <exception cref="InvalidOperationException">When the loan's <see cref="PrincipalEffectiveDate"/> is in the future.</exception>
        public decimal TotalCurrentlyOwed()
        {
            return AccruedInterestToDate() + Principal;
        }

        public Loan ProjectForward(DateTime to)
        {
            if (to < PrincipalEffectiveDate) throw new InvalidOperationException();

            var dateDiff = (to - PrincipalEffectiveDate).Days;

            if (dateDiff == 0) return this;


            var newPrincipal = 0.00m;
            if (to.Year == PrincipalEffectiveDate.Year)
            {
                newPrincipal = (CurrentDailyInterestRate() * (decimal)dateDiff) + Principal;
            }
            else
            {
                newPrincipal = ((PrincipalEffectiveDate.DaysInYear() - PrincipalEffectiveDate.DayOfYear) * CurrentDailyInterestRate()) + Principal;
                for (int i = PrincipalEffectiveDate.Year + 1; i < to.Year; i++)
                    newPrincipal *= (1 + InterestRate);

                newPrincipal += newPrincipal * (1 + InterestRate) / to.DaysInYear() * to.DayOfYear;
            }

            return new Loan(Id, newPrincipal, to) { InterestRate = InterestRate, LoanName = LoanName, MinimumPayment = MinimumPayment };
        }
    }
}
