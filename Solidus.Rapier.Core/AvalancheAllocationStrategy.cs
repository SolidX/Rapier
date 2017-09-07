using System;
using System.Collections.Generic;
using System.Linq;

namespace Solidus.Rapier.Core
{
    /// <summary>
    /// A Repayment strategy in which you pay an equal amount across all the loans in a bundle.
    /// It is also both the simplest and most naive way of doing things.
    /// </summary>
    public class AvalancheAllocationStrategy : IRepaymentStrategy
    {
        public Dictionary<int, Payment> RecommendedPaymentAllocations(IEnumerable<Loan> loans, decimal totalPayment, DateTime paymentDate)
        {
            var loansAsOfDt = loans.Select(l => l.ProjectForward(paymentDate)).OrderByDescending(x => x.InterestRate).ThenByDescending(x => x.Principal);  //Project debts to paymentDate & sort by highest interest rate first

            var leftOver = totalPayment - loansAsOfDt.Sum(x => x.MinimumPayment);
            var allocations = loansAsOfDt.ToDictionary(k => k.Id, v => new Payment { Amount = v.MinimumPayment, PaidOn = paymentDate});
            
            if (leftOver >= 0)
            {
                foreach (var l in loansAsOfDt)
                {
                    if (leftOver >= l.TotalOwed() - l.MinimumPayment)
                    {
                        leftOver = leftOver - (l.TotalOwed() - l.MinimumPayment);
                        allocations[l.Id].Amount = l.TotalOwed();
                    }
                    else
                    {
                        allocations[l.Id].Amount += leftOver;
                        leftOver = 0;
                        break;
                    }
                }
            }

            return allocations;
        }
    }
}
