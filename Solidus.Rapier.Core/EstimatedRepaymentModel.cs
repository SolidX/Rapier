using System;
using System.Collections.Generic;

namespace Solidus.Rapier.Core
{
    public class EstimatedRepaymentModel
    {
        public decimal TotalPaid { get; internal set; }

        public Dictionary<int, decimal> PaymentBreakdown { get; internal set; }
        public DateTime RepaymentDate { get; internal set; }
        public string StrategyName { get; internal set; }
    }
}
