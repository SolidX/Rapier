using System;
using System.Collections.Generic;
using System.Linq;

namespace Solidus.Rapier.Core
{
    public class ProportionalAllocationStrategy : IRepaymentStrategy
    {
        public Dictionary<int, Payment> RecommendedPaymentAllocations(IEnumerable<Loan> loans, decimal totalPayment, DateTime paymentDate)
        {
            var loansAsOfDt = loans.Select(l => l.ProjectForward(paymentDate)).ToDictionary(k => k.Id);  //accumulated interest to the 1st payment

            while (true)
            {
                //Loan Payment suggestion calculation
                var totalInterest = loansAsOfDt.Values.Sum(l => l.CurrentDailyInterestRate());
                var recommendations = loansAsOfDt.Values.Where(l => l.TotalOwed() > 0m).ToDictionary(k => k.Id, v => new Payment() { Amount = totalPayment * (v.CurrentDailyInterestRate() / totalInterest), PaidOn = paymentDate });

                foreach (var kvp in recommendations)
                {
                    //Meet minimum monthly payment requirement
                    var minPayment = loansAsOfDt[kvp.Key].MinimumPayment;
                    if (kvp.Value.Amount < minPayment)
                        kvp.Value.Amount = minPayment;

                    //Pay even less if the loan balance is less than the minimum payment
                    if (loansAsOfDt[kvp.Key].TotalOwed() <= loansAsOfDt[kvp.Key].MinimumPayment)
                        kvp.Value.Amount = loansAsOfDt[kvp.Key].TotalOwed();
                }

                return recommendations;
            }
        }
    }
}
