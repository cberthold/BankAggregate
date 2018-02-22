using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BankAggExample.Command;
using MediatR;

namespace BankAggExample.Application.Service
{

    public class BankManager : IBankManager
    {
        private readonly IMediator mediator;
        public BankManager(IMediator mediator)
        {
            this.mediator = mediator;
        }

        public async Task<Guid> CreateNewAccount()
        {
            var token = new CancellationToken();
            var command = new CreateNewAccountCommand();
            return await mediator.Send(command, token);
        }

        public async Task DepositAmount(Guid accountId, decimal amount)
        {
            var token = new CancellationToken();
            var command = new DepositAmountCommand(accountId, amount);
            await mediator.Send(command, token);
        }

        public async Task TransferFunds(Guid fromAccountId, Guid toAccountId, decimal amountToTransfer)
        {
            var token = new CancellationToken();
            var command = new TransferFundsCommand(fromAccountId, toAccountId, amountToTransfer);
            await mediator.Send(command, token);
        }

        public async Task WithdrawAmount(Guid accountId, decimal amount)
        {
            var token = new CancellationToken();
            var command = new WithdrawAmountCommand(accountId, amount);
            await mediator.Send(command, token);
        }
    }
}
