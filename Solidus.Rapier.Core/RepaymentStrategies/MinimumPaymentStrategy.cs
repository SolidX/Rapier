using System;
using System.Collections.Generic;
using System.Linq;

namespace Solidus.Rapier.Core
{
    /// <summary>
    /// A Repayment strategy in which you pay no more than the monthly minimum each month.
    /// It is both the simplest and most ineffective way of doing things.
    /// </summary>
    public class MinimumPaymentStrategy : IRepaymentStrategy
    {
        public Dictionary<int, Payment> RecommendedPaymentAllocations(IEnumerable<Loan> loans, decimal totalPayment, DateTime paymentDate)
        {
            var loansAsOfDt = loans.Select(l => l.ProjectForward(paymentDate));
            var remainingFunds = totalPayment - loansAsOfDt.Sum(x => x.EffeciveMinimumPayment);
            var noMinimumLoans = loansAsOfDt.Count(x => x.MinimumPayment <= 0m);
            return loansAsOfDt.ToDictionary(k => k.Id, v => new Payment { Amount = v.Principal > 0 ? v.EffeciveMinimumPayment : Math.Round(remainingFunds / noMinimumLoans), PaidOn = paymentDate });
        }
    }
}
