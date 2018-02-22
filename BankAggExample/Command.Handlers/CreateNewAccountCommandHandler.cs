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
    public class CreateNewAccountCommandHandler : IRequestHandler<CreateNewAccountCommand, Guid>
    {
        private readonly ISession session;
        public CreateNewAccountCommandHandler(ISession session)
        {
            this.session = session;
        }

        public async Task<Guid> Handle(CreateNewAccountCommand request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Bank Manager new account");
            // create new account
            var account = AccountAggregate.StartNewAccount();
            // add to session
            await session.Add(account, cancellationToken);
            // save
            await session.Commit(cancellationToken);

            Console.WriteLine($"Bank Manager completed CreateNewAccount Balance: {account.CurrentAccountBalance}");
            // return new id
            return account.Id;
        }
    }
}
