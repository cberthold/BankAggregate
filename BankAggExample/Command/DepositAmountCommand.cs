using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace BankAggExample.Command
{
    public class DepositAmountCommand : IRequest
    {
        public Guid AccountId { get; }
        public decimal Amount { get; }

        public DepositAmountCommand(Guid accountId, decimal amount)
        {
            this.AccountId = accountId;
            this.Amount = amount;
        }
    }
}
