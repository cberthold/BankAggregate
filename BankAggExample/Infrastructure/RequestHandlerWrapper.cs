using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace BankAggExample.Infrastructure
{
    internal class RequestHandlerWrapper<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IRequestHandler<TRequest, TResponse> _innerHandler;

        public RequestHandlerWrapper(IRequestHandler<TRequest, TResponse> innerHandler)
        {
            _innerHandler = innerHandler;
        }

        public Task<TResponse> Handle(TRequest message, CancellationToken cancellationToken)
        {
            return _innerHandler.Handle(message, cancellationToken);
        }
    }
}
