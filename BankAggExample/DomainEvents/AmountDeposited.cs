using System;
using System.Collections.Generic;
using System.Text;
using CQRSlite.Events;

namespace BankAggExample.DomainEvents
{
    public class AmountDeposited : IEvent
    {
        #region IEvent props

        public Guid Id { get; set; }
        public int Version { get; set; }
        public DateTimeOffset TimeStamp { get; set; }

        #endregion

        public decimal Amount { get; }

        public AmountDeposited(decimal amount)
        {
            Amount = amount;
        }
    }
}
