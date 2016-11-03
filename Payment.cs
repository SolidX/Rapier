using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanRepaymentProjector
{
    public struct Payment
    {
        public decimal Amount { get; set; }
        public DateTime PaidOn { get; set; }
    }
}
