using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace BankAggExample.Command
{
    public class WithdrawAmountCommand : IRequest
    {
        public Guid AccountId { get; }
        public decimal Amount { get; }

        public WithdrawAmountCommand(Guid accountId, decimal amount)
        {
            this.AccountId = accountId;
            this.Amount = amount;
        }
    }
}
