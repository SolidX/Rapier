using System;
using System.Collections.Generic;

namespace Solidus.Rapier.Core
{
    public interface IRepaymentStrategy
    {
        /// <summary>
        /// Calculates a recommended allocation of a payment across all the loans in the same bundle.
        /// </summary>
        /// <param name="loans">All of the loans that the <paramref name="totalPayment"/> will be applied to.</param>
        /// <param name="totalPayment">The amount of payment to allocate across all of the <paramref name="loans"/></param>
        /// <param name="paymentDate">The date to calculate the recommended payment amounts.</param>
        /// <returns>A dictionary maping loan ids to recommended payment amounts.</returns>
        Dictionary<int, Payment> RecommendedPaymentAllocations(IEnumerable<Loan> loans, decimal totalPayment, DateTime paymentDate);
    }
}
