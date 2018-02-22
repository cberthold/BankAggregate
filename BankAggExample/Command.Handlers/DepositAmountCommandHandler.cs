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
    public class DepositAmountCommandHandler : IRequestHandler<DepositAmountCommand>
    {
        private readonly ISession session;
        public DepositAmountCommandHandler(ISession session)
        {
            this.session = session;
        }

        public async Task Handle(DepositAmountCommand message, CancellationToken cancellationToken)
        {
            var amount = message.Amount;
            var accountId = message.AccountId;

            Console.WriteLine($"Bank Manager deposit amount ${amount}");
            var account = await session.Get<AccountAggregate>(accountId, null, cancellationToken);
            account.Deposit(amount);
            await session.Commit(cancellationToken);
            Console.WriteLine($"Bank Manager completed DepositAmount Balance: {account.CurrentAccountBalance}");
        }
    }
}
