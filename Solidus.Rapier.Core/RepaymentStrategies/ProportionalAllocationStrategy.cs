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
                var totalInterest = 0m;
                var totalOwed = 0m;
                
                //Sum daily accruing interest and balances owed
                foreach (var l in loansAsOfDt.Values)
                {
                    totalInterest += l.CurrentDailyInterestRate();
                    totalOwed += l.TotalOwed();
                }

                //Loan Payment suggestion calculation
                var recommendations = loansAsOfDt.Values.Where(l => l.TotalOwed() > 0m).ToDictionary(k => k.Id, v =>
                {
                    var suggestedPayment = v.MinimumPayment;

                    if (totalInterest == 0m)
                    {
                        //In the special case where all loans are not accruing interest, allocate proprotional to balance owed rather than the daily interest accrued.
                        //Thanks COVID-19 for putting everyone's loans in to forebearance!
                        var balanceBasedPayment = totalPayment * (v.TotalOwed() / totalOwed);
                        suggestedPayment = (balanceBasedPayment >= v.MinimumPayment ? balanceBasedPayment : v.MinimumPayment); //Meet minimum monthly payment requirement

                    }
                    else
                    {
                        var interestBasedPayment = totalPayment * (v.CurrentDailyInterestRate() / totalInterest);
                        suggestedPayment = (interestBasedPayment >= v.MinimumPayment ? interestBasedPayment : v.MinimumPayment); //Meet minimum monthly payment requirement
                    }

                    //Reduce suggested payment amount to match loan balance if it exceeds the loan's balance
                    suggestedPayment = (suggestedPayment > v.TotalOwed() ? v.TotalOwed() : suggestedPayment);

                    return new Payment() { Amount = suggestedPayment, PaidOn = paymentDate }; ;
                });

                return recommendations;
            }
        }
    }
}
