using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace BankAggExample.Command
{
    public class CreateNewAccountCommand : IRequest<Guid>
    {
    }
}
