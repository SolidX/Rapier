using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanRepaymentProjector
{
    public class Loan
    {
        private static int _idGenerator = 0;
        private decimal _principal;
        private DateTime _principalEffectiveDate;

        public readonly int Id;
        public decimal InterestRate { get; set; }
        public decimal Principal
        {
            get { return _principal; }
        }
        public decimal MinimumPayment { get; set; }        
        public string Name { get; set; }

        public Loan()
        {
            Id = _idGenerator;
            _idGenerator++;
        }

        private Loan(int id, decimal principal, DateTime principalDt)
        {
            Id = id;
            _principal = principal;
            _principalEffectiveDate = principalDt;
        }

        public void SetPrincipal(decimal amount, DateTime asOf)
        {
            _principal = amount;
            _principalEffectiveDate = asOf;
        }
        public decimal CurrentDailyInterestRate()
        {
            return (_principal * InterestRate) / (decimal) _principalEffectiveDate.DaysInYear();
        }

        public Loan ProjectForward(DateTime to)
        {
            if (to < _principalEffectiveDate) throw new InvalidOperationException();

            var dateDiff = (to - _principalEffectiveDate).Days;

            if (dateDiff == 0) return this;


            var newPrincipal = 0.00m;
            if (to.Year == _principalEffectiveDate.Year)
            {
                newPrincipal = (CurrentDailyInterestRate() * (decimal)dateDiff) + Principal;
            }
            else
            {
                newPrincipal = ((_principalEffectiveDate.DaysInYear() - _principalEffectiveDate.DayOfYear) * CurrentDailyInterestRate()) + Principal;
                for (int i = _principalEffectiveDate.Year + 1; i < to.Year; i++)
                    newPrincipal *= (1 + InterestRate);

                newPrincipal += newPrincipal * (1 + InterestRate) / to.DaysInYear() * to.DayOfYear;
            }

            return new Loan(Id, newPrincipal, to) { InterestRate = InterestRate, Name = Name, MinimumPayment = MinimumPayment };
        }

        //public Loan ProjectForward(DateTime to, IEnumerable<Payment> payments)
        //{
        //    if (to < _principalEffectiveDate) throw new NotImplementedException();

        //    var dateDiff = (to - _principalEffectiveDate).Days;

        //    if (dateDiff == 0) return this;

        //    var newPrincipal = CurrentDailyInterestRate() * (decimal)dateDiff;

        //    if (to.Year != _principalEffectiveDate.Year)
        //    {
        //        for (int i = _principalEffectiveDate.Year + 1; i < to.Year; i++)
        //            newPrincipal *= InterestRate;

        //        newPrincipal = newPrincipal * InterestRate / to.DaysInYear() * to.DayOfYear;
        //    }

        //    return new Loan(Id, newPrincipal, to) { InterestRate = InterestRate, Name = Name, MinimumPayment = MinimumPayment };
        //}
    }
}
