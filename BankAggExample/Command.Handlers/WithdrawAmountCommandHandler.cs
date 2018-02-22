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
    public class WithdrawAmountCommandHandler : IRequestHandler<WithdrawAmountCommand>
    {
        private readonly ISession session;
        public WithdrawAmountCommandHandler(ISession session)
        {
            this.session = session;
        }

        public async Task Handle(WithdrawAmountCommand message, CancellationToken cancellationToken)
        {
            var amount = message.Amount;
            var accountId = message.AccountId;

            Console.WriteLine($"Bank Manager withdraw amount ${amount}");
            var account = await session.Get<AccountAggregate>(accountId, null, cancellationToken);
            account.Withdraw(amount);
            await session.Commit(cancellationToken);
            Console.WriteLine($"Bank Manager completed WithdrawAmount Balance: {account.CurrentAccountBalance}");
        }
    }
}
