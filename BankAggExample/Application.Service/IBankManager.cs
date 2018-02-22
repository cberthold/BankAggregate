using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BankAggExample.Application.Service
{
    public interface IBankManager
    {
        Task<Guid> CreateNewAccount();
        Task DepositAmount(Guid accountId, decimal amount);
        Task WithdrawAmount(Guid accountId, decimal amount);
        Task TransferFunds(Guid fromAccountId, Guid toAccountId, decimal amountToTransfer);
    }
}
