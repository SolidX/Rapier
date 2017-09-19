using System;
using System.Collections.Generic;
using System.Linq;

namespace Solidus.Rapier.Core
{
    /// <summary>
    /// A Repayment strategy in which you pay an equal amount across all the loans in a bundle.
    /// It is also both the simplest and most naive way of doing things.
    /// </summary>
    public class EqualAllocationStrategy : IRepaymentStrategy
    {
        public Dictionary<int, Payment> RecommendedPaymentAllocations(IEnumerable<Loan> loans, decimal totalPayment, DateTime paymentDate)
        {
            var loansAsOfDt = loans.Select(l => l.ProjectForward(paymentDate)).ToDictionary(k => k.Id);  //Project debts to paymentDate
            var allocation = totalPayment / loansAsOfDt.Values.Count(x => x.TotalOwed() > 0);
            return loansAsOfDt.Values.ToDictionary(k => k.Id, v => new Payment { Amount = v.TotalOwed() > 0 ? allocation : 0, PaidOn = paymentDate });
        }
    }
}
