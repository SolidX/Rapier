using System;
using System.Collections.Generic;

namespace LoanRepaymentProjector
{
    public class Loan
    {
        private static int _idGenerator = 0;

        public readonly int Id;

        /// <summary>
        /// The amount of interest accrued on the loan
        /// </summary>
        public decimal AccruedInterest { get; private set; }

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
        /// Deprecated. This is only here for backwards compatability at the moment. Specify the accrued interest going forward.
        /// </summary>
        /// <param name="principal">The loan's principal balance</param>
        /// <param name="asOf">The date os of which the <paramref name="principal"/> are being reported.</param>
        [Obsolete("Specify the accrued interest as well going foward.")]
        public void SetBalance(decimal principal, DateTime asOf)
        {
            Principal = principal;
            PrincipalEffectiveDate = asOf;
        }

        /// <summary>
        /// Sets the <see cref="Principal"/> balance & <see cref="AccruedInterest"/> of the loan and its associated <see cref="PrincipalEffectiveDate"/>
        /// </summary>
        /// <param name="principal">The loan's principal balance</param>
        /// <param name="interest">The accrued interest on the loan</param>
        /// <param name="asOf">The date as of which the <paramref name="principal"/> and accrued <paramref name="interest"/> are being set.</param>
        public void SetBalance(decimal principal, decimal interest, DateTime asOf)
        {
            Principal = principal;
            AccruedInterest = interest;
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
        public decimal CalculateInterest(DateTime asOf)
        {
            if (asOf < PrincipalEffectiveDate) throw new InvalidOperationException();

            var total = 0.00m;

            for (var i = PrincipalEffectiveDate.Year; i <= asOf.Year; i++)
            {
                if (i == PrincipalEffectiveDate.Year)
                {
                    var dateDiff = 0;

                    if (PrincipalEffectiveDate.Year == asOf.Year)
                        dateDiff = (asOf - PrincipalEffectiveDate).Days;
                    else
                        dateDiff = (new DateTime(PrincipalEffectiveDate.Year, 12, 31) - PrincipalEffectiveDate).Days;

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
                total += (Principal * InterestRate);
            }

            return total;
        }

        /// <summary>
        /// The sum of the interest accrued up until <paramref name="asOf"/> and the <see cref="Principal"/> balance.
        /// </summary>
        /// <param name="asOf">The date to calculate accrued interest for</param>
        /// <returns>The total balance owed on the loan as of the provided date.</returns>
        /// <exception cref="InvalidOperationException">When the provided <paramref name="asOf"/> date is before the loan's <see cref="PrincipalEffectiveDate"/>.</exception>
        public decimal TotalOwed(DateTime asOf)
        {
            return CalculateInterest(asOf) + Principal;
        }

        /// <summary>
        /// The sum of <see cref="AccruedInterest"/> and <see cref="Principal"/>
        /// </summary>
        /// <returns>The total balance currently owed on the loan.</returns>
        /// <exception cref="InvalidOperationException">When the loan's <see cref="PrincipalEffectiveDate"/> is in the future.</exception>
        public decimal TotalOwed()
        {
            return AccruedInterest + Principal;
        }

        /// <summary>
        /// Returns a copy of this loan with the payment amount applied to the <see cref="AccruedInterest"/> first and then the <see cref="Principal"/>.
        /// </summary>
        /// <param name="p">The payment to make.</param>
        /// <returns>A new Loan reflecting the payment on the principal balance & accrued interest</returns>
        /// <exception cref="InvalidOperationException">When the provided Payment's PaidOn date is before the <see cref="PrincipalEffectiveDate"/>.</exception>
        public Loan MakePayment(Payment p)
        {
            if (p.PaidOn < PrincipalEffectiveDate) throw new InvalidOperationException();

            var paymentAmount = p.Amount;
            var interestReduction = 0.00m;
            var principalReduction = 0.00m;

            if (paymentAmount >= AccruedInterest)
            {
                interestReduction = AccruedInterest;
                paymentAmount -= AccruedInterest;
            }
            else
            {
                interestReduction = paymentAmount;
                paymentAmount = 0;
            }

            principalReduction = paymentAmount;

            var l = new Loan { InterestRate = InterestRate, LoanName = LoanName, MinimumPayment = MinimumPayment };
            l.SetBalance(Principal - principalReduction, AccruedInterest - interestReduction, p.PaidOn);
            return l;
        }

        /// <summary>
        /// Returns a copy of this loan with no payments applied at the provided date.
        /// </summary>
        /// <param name="to">Date to project the loan balance to.</param>
        /// <returns>A new Loan reflecting an accumulatin of interest over time.</returns>
        /// <exception cref="InvalidOperationException">When the provided date is before the <see cref="PrincipalEffectiveDate"/>.</exception>
        public Loan ProjectForward(DateTime to)
        {
            if (to < PrincipalEffectiveDate) throw new InvalidOperationException();
            if ((to - PrincipalEffectiveDate).Days == 0) return this;

            var l = new Loan { InterestRate = InterestRate, LoanName = LoanName, MinimumPayment = MinimumPayment };
            l.SetBalance(Principal, AccruedInterest + Math.Round(CalculateInterest(to), 2), to);
            return l;
        }
    }
}
