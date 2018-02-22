using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace BankAggExample.Command
{
    public class TransferFundsCommand : IRequest
    {
        public Guid FromAccountId { get; }
        public Guid ToAccountId { get; }
        public decimal AmountToTransfer { get; }

        public TransferFundsCommand(Guid fromAccountId, Guid toAccountId, decimal amountToTransfer)
        {
            this.FromAccountId = fromAccountId;
            this.ToAccountId = toAccountId;
            this.AmountToTransfer = amountToTransfer;
        }
    }
}
