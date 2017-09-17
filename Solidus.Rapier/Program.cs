using Solidus.Rapier.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Solidus.Rapier
{
    class Program
    {
        private static Dictionary<int, Loan> allLoans;
        private enum LoanWarnings
        {
            LargestDebt,
            HighestInterestRate,
            HighestDailyRate,
        }

        static void Main(string[] args)
        {
            allLoans = GetLoans().ToDictionary(l => l.Id);

            DisplaySplashLogo();
            Console.WriteLine("\n");
        }

        public static List<Loan> GetLoans()
        {
            var loans = new List<Loan>();
            return loans;
        }

        }
        }

        /// <summary>
        /// Outputs loan information to the console in tabular form.
        /// Displays the loans with the largest balances, fastest accumulating intrest and highest interest rates with additional emphasis.
        /// </summary>
        /// <param name="loanSet">The loans to display</param>
        public static void DisplayLoansStatus(IEnumerable<Loan> loanSet)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("-----------------------------------------------------------");
            Console.WriteLine("{0,-3:G} | {1,-10:G} |{2,-6:G}| {3,-10:G} | {4,-7:G} | {5,-7:G}", "Id", "Name", "Int. Rate", "Principal", "Acc Int", "Min Pay");
            Console.WriteLine("----+------------+---------+------------+---------+--------");

            var largestDebt = loanSet.Where(l => l.Principal == loanSet.Max(m => m.Principal)).Select(l => l.Id);
            var highestInterest = loanSet.Where(l => l.InterestRate == loanSet.Max(m => m.InterestRate)).Select(l => l.Id);
            var highestDaily = loanSet.Where(l => l.CurrentDailyInterestRate() == loanSet.Max(m => m.CurrentDailyInterestRate())).Select(l => l.Id);

            var idsToWarn = largestDebt.Union(highestInterest).Distinct();

            foreach (var loan in loanSet)
            {
                var criticalWarning = highestDaily.Contains(loan.Id);
                if (criticalWarning) Console.BackgroundColor = ConsoleColor.DarkRed;

                if (idsToWarn.Contains(loan.Id))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("{0,3:G}", loan.Id);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(" | ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("{0,10}", loan.LoanName);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(" | ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("{0,6:F3}%", loan.InterestRate * 100);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(" | ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("{0,10:C}", loan.Principal);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(" | ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("{0,7:C}", loan.AccruedInterest);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(" | ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("{0,7:C}", loan.MinimumPayment);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.WriteLine("{0,3:G} | {1,10} | {2,6:F3}% | {3,10:C} | {4,7:C} | {5,7:C}", loan.Id, loan.LoanName, loan.InterestRate * 100, loan.Principal, loan.AccruedInterest, loan.MinimumPayment);
                }

                if (criticalWarning) Console.BackgroundColor = ConsoleColor.Black;
            }
            Console.WriteLine();
            Console.ResetColor();
        }

        public static void DisplaySplashLogo()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(@"");
            Console.WriteLine(@"====================================================");
            Console.WriteLine(@"           ____                 _                   ");
            Console.WriteLine(@"          / __ \ ____ _ ____   (_)___   _____       ");
            Console.WriteLine(@"         / /_/ // __ `// __ \ / // _ \ / ___/       ");
            Console.WriteLine(@"        / _, _// /_/ // /_/ // //  __// /           ");
            Console.WriteLine(@"       /_/ |_| \__,_// .___//_/ \___//_/            ");
            Console.WriteLine(@"                    /_/                             ");
            Console.WriteLine(@"     ~The Loan Repayment Decision Support Tool~     ");
            Console.WriteLine(@"====================================================");
            Console.WriteLine(@"");
            Console.ResetColor();
        }

        /// <summary>
        /// Given a total amount to pay in to all loans, displays a recommended amount that should be paid into each Loan based on accumulating interest.
        /// </summary>
        /// <param name="totalFunds">Total amount to pay in to all loans for this payment period.</param>
        /// <returns></returns>
        public static void GetCurrentRecommendedPaymentAmount(decimal totalFunds)
        {
            var dt = DateTime.Now;
            var loansAsOfNow = GetLoanProjections(allLoans.Values, dt);
            var totalInterest = loansAsOfNow.Sum(kvp => kvp.Value.CurrentDailyInterestRate());
            var recommendations = new Dictionary<Loan, Payment>();

            foreach (var loan in loansAsOfNow.Values)
            {
                var recommendedAmount = totalFunds * (loan.CurrentDailyInterestRate() / totalInterest);
                var recommendedPayment = new Payment();
                recommendedPayment.Amount = recommendedAmount > loan.MinimumPayment ? recommendedAmount : loan.MinimumPayment;
                recommendations.Add(loan, recommendedPayment);
            }
            //TODO: Ensure min payment is met by scaling down total payment if it's greater than totalFunds

            Console.WriteLine("Recommended Payments on " + dt.ToString());
            DisplayRecommendedLoanPayment(recommendations);
        }

        public static void DisplayRecommendedLoanPayment(Dictionary<Loan, Payment> recommendations)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine("{0,-3:G} | {1,-10:G} | {2,-10:G} | {3,-8:G} | {4,-7:G}", "Id", "Name", "Principal", "Interest", "Payment");
            Console.WriteLine("----+------------+------------+----------+--------");

            foreach (var rec in recommendations)
            {
                Console.WriteLine("{0,3:G} | {1,10} | {2,10:C} | {3,8:C} | {4,7:C}", rec.Key.Id, rec.Key.LoanName, rec.Key.Principal, rec.Key.AccruedInterest, rec.Value.Amount);
            }
            Console.WriteLine();
            Console.ResetColor();
        }

        /// <summary>
        /// Projects how long it will take to repay all loans with a fixed monthly payment.
        /// </summary>
        /// <param name="b">Loans to estimate repayment completion date for.</param>
        /// <param name="avgMonthlyPayment">Amount to pay each month</param>
        /// <param name="firstPayment">Date of the first payment</param>
        /// <returns>Last payment date for the loans</returns>
        public static DateTime EstimateRepaymentCompletionDate(LoanBundle b, decimal avgMonthlyPayment, DateTime firstPayment)
        {
            if (avgMonthlyPayment <= 0)
                return DateTime.MaxValue;

            return b.EstimateRepaymentCompletionDate(avgMonthlyPayment, firstPayment);
        }
    }
}
