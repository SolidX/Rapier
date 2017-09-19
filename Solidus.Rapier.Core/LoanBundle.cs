using System;
using System.Linq;
using System.Collections.Generic;

namespace Solidus.Rapier.Core
{
    public class LoanBundle : ICloneable
    {
        public IEnumerable<Loan> Loans { get; set; }
        public IRepaymentStrategy RepaymentStrategy { get; private set; }

        public LoanBundle(IRepaymentStrategy strat)
        {
            RepaymentStrategy = strat;
        }

        /// <summary>
        /// Given a total amount to pay in to all loans, displays a recommended amount that should be paid into each Loan based on the repayment strategy.
        /// </summary>
        /// <param name="totalFunds">Total amount to pay in to all loans for this payment period.</param>
        /// <param name="dt">Date to generate a recommendation for.</param>
        /// <returns>A dictionary of loan Id to Payment re</returns>
        public Dictionary<int, Payment> GetRecommendedPaymentAmounts(decimal totalFunds, DateTime dt)
        {
            return RepaymentStrategy.RecommendedPaymentAllocations(Loans, totalFunds, dt);
        }

        public DateTime EstimateRepaymentCompletionDate(decimal avgMonthlyPayment, DateTime firstPayment)
        {
            if (avgMonthlyPayment <= 0)
                return DateTime.MaxValue;

            var now = firstPayment;
            var loans = Loans.ToDictionary(k => k.Id);

            while (loans.Values.Sum(x => x.TotalOwed()) > 0)
            {
                var recommendations = RepaymentStrategy.RecommendedPaymentAllocations(loans.Values, avgMonthlyPayment, now);
                foreach (var x in recommendations.Keys)
                    loans[x].MakePayment(recommendations[x]);

                now = now.AddMonths(1);
            }

            return now;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="avgMonthlyPayment"></param>
        /// <param name="firstPayment"></param>
        /// <returns></returns>
        public EstimatedRepaymentModel EstimateRepaymentDetails(decimal avgMonthlyPayment, DateTime firstPayment)
        {
            var now = firstPayment;
            var loans = Loans.ToDictionary(k => k.Id);
            var lifetimePayements = loans.Keys.ToDictionary(k => k, v => 0m);

            while (loans.Values.Sum(x => x.TotalOwed()) > 0)
            {
                var recommendations = RepaymentStrategy.RecommendedPaymentAllocations(loans.Values, avgMonthlyPayment, now);
                foreach (var x in recommendations.Keys)
                {
                    loans[x].MakePayment(recommendations[x]);
                    lifetimePayements[x] += recommendations[x].Amount;
                }

                now = now.AddMonths(1);
            }

            return new EstimatedRepaymentModel { StrategyName = RepaymentStrategy.GetType().Name.ToString(), RepaymentDate = now, TotalPaid = lifetimePayements.Values.Sum(), PaymentBreakdown = lifetimePayements };
        }

        public object Clone()
        {   
            var l = new List<Loan>();
            l.AddRange(Loans.Select(x => x.Clone() as Loan));

            return new LoanBundle(RepaymentStrategy) { Loans = l };
        }
    }
}
