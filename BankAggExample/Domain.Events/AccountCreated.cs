using System;
using System.Collections.Generic;
using System.Text;
using CQRSlite.Events;

namespace BankAggExample.Domain.Events
{
    public class AccountCreated : IEvent
    {
        #region IEvent props

        public Guid Id { get; set; }
        public int Version { get; set; }
        public DateTimeOffset TimeStamp { get; set; }

        #endregion

        public decimal DepositAmount { get; }
        public DateTime DateOpened { get; }

        public AccountCreated(decimal depositAmount, DateTime dateOpened)
        {
            DepositAmount = depositAmount;
            DateOpened = dateOpened;
        }
    }
}
