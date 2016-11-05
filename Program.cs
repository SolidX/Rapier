using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanRepaymentProjector
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
            GetCurrentRecommendedPaymentAmount(400m);
        public static List<Loan> GetLoans()
        {
            var principalDt = new DateTime(2016, 10, 13);
            var loans = new List<Loan>();
            return loans;
        }

            return loans;
        }
        public static void DisplayLoansStatus(Dictionary<int, Loan> loanSet)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine("{0,-3:G} | {1,-10:G} |{2,-6:G}| {3,-10:G} | {4,-7:G}", "Id", "Name", "Int. Rate", "Principal", "Min Pay");
            Console.WriteLine("----+------------+---------+------------+--------");

            var largestDebt = loanSet.Values.Where(l => l.Principal == loanSet.Values.Max(m => m.Principal)).Select(l => l.Id);
            var highestInterest = loanSet.Values.Where(l => l.InterestRate == loanSet.Values.Max(m => m.InterestRate)).Select(l => l.Id);
            var highestDaily = loanSet.Values.Where(l => l.CurrentDailyInterestRate() == loanSet.Values.Max(m => m.CurrentDailyInterestRate())).Select(l => l.Id);

            var idsToWarn = largestDebt.Union(highestInterest).Distinct();

            foreach (var loan in loanSet.Values)
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
                    Console.WriteLine("{0,7:C}", loan.MinimumPayment);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.WriteLine("{0,3:G} | {1,10} | {2,6:F3}% | {3,10:C} | {4,7:C}", loan.Id, loan.LoanName, loan.InterestRate * 100, loan.Principal, loan.MinimumPayment);
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

        public static Dictionary<int, Loan> GetLoanProjections(IEnumerable<Loan> loans, DateTime dt)
        {
            return loans.ToDictionary(k => k.Id, v => v.ProjectForward(dt));
        }

        public static void ProjectForwardTo(DateTime dt, decimal paid)
        {
            var testProjection = GetLoanProjections(allLoans.Values, dt);

            Console.WriteLine("Projection to " + dt.ToString());
            DisplayLoansStatus(testProjection);
        }

        public static void DisplayRecommendedLoanPayment(Dictionary<Loan, Payment> recommendations)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("---------------------------------------");
            Console.WriteLine("{0,-3:G} | {1,-10:G} | {2,-10:G} | {3,-7:G}", "Id", "Name", "Principal", "Payment");
            Console.WriteLine("----+------------+------------+--------");

            foreach (var rec in recommendations)
            {
                Console.WriteLine("{0,3:G} | {1,10} | {2,10:C} | {3,7:C}", rec.Key.Id, rec.Key.LoanName, rec.Key.Principal, rec.Value.Amount);
            }
            Console.WriteLine();
            Console.ResetColor();
        }

        public static DateTime EstimateRepaymentCompletionDate(decimal avgMonthlyPayment, DateTime firstPayment)
        {
            if (avgMonthlyPayment <= 0)
                return DateTime.MaxValue;

            var now = firstPayment;
            var loans = allLoans;

            var totalPrincipal = loans.Values.Sum(l => l.Principal);
            var totalPaidIn = 0m;

            do
            {
                loans = loans.Values.Select(l => l.ProjectForward(now)).ToDictionary(k => k.Id);

                //Loan Payment suggestion calculation
                //TODO this architehcture is ass, fix it
                var totalInterest = loans.Values.Sum(l => l.CurrentDailyInterestRate());
                var recommendations = new Dictionary<int, Payment>();

                foreach (var loan in loans.Values)
                {
                    var recommendedAmount = avgMonthlyPayment * (loan.CurrentDailyInterestRate() / totalInterest);
                    if (recommendedAmount <= 0m && loan.Principal <= 0m) continue;
                    var recommendedPayment = new Payment();
                    recommendedPayment.Amount = recommendedAmount > loan.MinimumPayment ? recommendedAmount : loan.MinimumPayment;
                    recommendations.Add(loan.Id, recommendedPayment);

                    //Apply recommended payment
                    loan.SetBalance(loan.Principal - recommendedPayment.Amount, now);
                    totalPaidIn += recommendedPayment.Amount;
                }
                now = now.AddMonths(1);
            }
            while (loans.Values.Sum(l => l.Principal) > 0m);

            return now;
        }
    }
}
