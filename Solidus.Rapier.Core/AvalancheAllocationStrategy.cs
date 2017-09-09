using System;
using System.Collections.Generic;
using System.Linq;

namespace Solidus.Rapier.Core
{
    /// <summary>
    /// A Repayment strategy in which you pay the minimum payment in to each loan and then apply any money left over to the loan(s) with the highest interest rate.
    /// This method is known as debt-stacking or the avalanche model.
    /// </summary>
    public class AvalancheAllocationStrategy : IRepaymentStrategy
    {
        public Dictionary<int, Payment> RecommendedPaymentAllocations(IEnumerable<Loan> loans, decimal totalPayment, DateTime paymentDate)
        {
            var loansAsOfDt = loans.Select(l => l.ProjectForward(paymentDate)).OrderByDescending(x => x.InterestRate).ThenByDescending(x => x.Principal);  //Project debts to paymentDate & sort by highest interest rate first

            var leftOver = totalPayment - loansAsOfDt.Sum(x => x.EffeciveMinimumPayment);
            var allocations = loansAsOfDt.ToDictionary(k => k.Id, v => new Payment { Amount = v.EffeciveMinimumPayment, PaidOn = paymentDate});
            
            if (leftOver >= 0)
            {
                foreach (var l in loansAsOfDt)
                {
                    if (leftOver >= l.TotalOwed() - l.EffeciveMinimumPayment)
                    {
                        leftOver = leftOver - (l.TotalOwed() - l.EffeciveMinimumPayment);
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
