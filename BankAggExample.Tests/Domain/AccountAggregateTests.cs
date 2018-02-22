using System;
using System.Linq;
using BankAggExample.Domain;
using BankAggExample.Domain.Events;
using Xunit;

namespace BankAggExample.Tests.Domain
{
    public class AccountAggregateTests
    {
        [Fact]
        public void given_new_account_should_have_create_event_with_non_empty_id()
        {
            // assemble

            // apply
            var newAgg = AccountAggregate.StartNewAccount();

            // assert
            var changes = newAgg.GetUncommittedChanges();
            Assert.Single(changes);
            Assert.Collection(changes, (e) =>
            {
                Assert.IsType<AccountCreated>(e);
                var @event = (AccountCreated)e;
                Assert.Equal(0, @event.DepositAmount);
            });

            Assert.NotEqual(Guid.Empty, newAgg.Id);
            Assert.Equal(0, newAgg.StartingBalance);
        }

        [Fact]
        public void given_new_account_should_show_balance_and_beginning_if_deposit_made()
        {
            // assemble
            const decimal AMOUNT_TO_DEPOSIT = 10;

            var newAgg = AccountAggregate.StartNewAccount();

            // apply
            newAgg.Deposit(AMOUNT_TO_DEPOSIT);

            // assert
            var changes = newAgg.GetUncommittedChanges();
            Assert.Equal(2, changes.Count());
            Assert.Collection(changes,
                (e) =>
                {
                    Assert.IsType<AccountCreated>(e);
                },
                (e) =>
                {
                    Assert.IsType<AmountDeposited>(e);
                    var @event = (AmountDeposited)e;
                    Assert.Equal(AMOUNT_TO_DEPOSIT, @event.Amount);
                });
            
            Assert.Equal(AMOUNT_TO_DEPOSIT, newAgg.StartingBalance);
            Assert.Equal(AMOUNT_TO_DEPOSIT, newAgg.CurrentAccountBalance);
        }
    }
}
