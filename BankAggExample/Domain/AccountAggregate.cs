using System;
using System.Collections.Generic;
using System.Text;
using BankAggExample.Domain.Events;
using CQRSlite.Domain;

namespace BankAggExample.Domain
{
    public class AccountAggregate : AggregateRoot
    {
        public decimal StartingBalance { get; private set; } = 0;
        public decimal CurrentAccountBalance { get; private set; } = 0;
        public DateTime DateAccountOpened { get; private set; }

        // has to have parameterless constructor
        private AccountAggregate() { }

        protected AccountAggregate(Guid accountId, DateTime dateCreated)
        {
            Id = accountId;
            ApplyChange(new AccountCreated(0, dateCreated));
        }

        public static AccountAggregate StartNewAccount()
        {
            var accountId = Guid.NewGuid();
            var creationDate = DateTime.Today;
            return new AccountAggregate(accountId, creationDate);
        }

        public void Deposit(decimal amountToDeposit)
        {
            ApplyChange(new AmountDeposited(amountToDeposit));
        }

        public void Transfer(AccountAggregate toAccount, decimal amountToTransfer)
        {
            if (CurrentAccountBalance < amountToTransfer)
            {
                throw new Exception($"Not enough money {CurrentAccountBalance} to transfer {amountToTransfer}");
            }

            var thisAccount = this;

            thisAccount.Withdraw(amountToTransfer);
            toAccount.Deposit(amountToTransfer);
        }

        public void Withdraw(decimal amountToWithdraw)
        {
            if (StartingBalance <= 0)
            {
                throw new Exception("Not allow to start withdrawing money without having some money deposited first");
            }

            var currentBalance = CurrentAccountBalance;
            var futureBalance = currentBalance - amountToWithdraw;

            if (futureBalance < -100)
            {
                throw new Exception("You can only overdraft up to $100");
            }


            ApplyChange(new AmountWithdrawn(amountToWithdraw));
        }

        private void Apply(AccountCreated @event)
        {
            StartingBalance = @event.DepositAmount;
            DateAccountOpened = @event.DateOpened;
        }

        private void Apply(AmountDeposited @event)
        {
            // if we didnt open the account with a deposit - our first deposit is our starting balance
            if (StartingBalance == 0)
            {
                StartingBalance = @event.Amount;
            }

            CurrentAccountBalance += @event.Amount;
        }

        private void Apply(AmountWithdrawn @event)
        {
            CurrentAccountBalance -= @event.Amount;
        }


    }
}
