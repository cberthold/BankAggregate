using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BankAggExample.Domain;
using CQRSlite.Domain;
using MediatR;

namespace BankAggExample.Command.Handlers
{
    public class TransferFundsCommandHandler : IRequestHandler<TransferFundsCommand>
    {
        private readonly ISession session;
        public TransferFundsCommandHandler(ISession session)
        {
            this.session = session;
        }

        public async Task Handle(TransferFundsCommand message, CancellationToken cancellationToken)
        {
            var amountToTransfer = message.AmountToTransfer;
            var fromAccountId = message.FromAccountId;
            var toAccountId = message.ToAccountId;

            Console.WriteLine($"Bank Manager transfer amount ${amountToTransfer}");
            var fromAccount = await session.Get<AccountAggregate>(fromAccountId, null, cancellationToken);
            var toAccount = await session.Get<AccountAggregate>(toAccountId, null, cancellationToken);
            fromAccount.Transfer(toAccount, amountToTransfer);
            await session.Commit(cancellationToken);
            Console.WriteLine($"Bank Manager completed DepositAmount from account balance: {fromAccount.CurrentAccountBalance} to account balance {toAccount.CurrentAccountBalance}");

        }
    }
}
