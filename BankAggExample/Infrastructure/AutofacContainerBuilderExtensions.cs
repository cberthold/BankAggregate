using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Autofac;
using Autofac.Core;
using Autofac.Features.Variance;
using BankAggExample.Infrastructure;
using MediatR;

namespace Autofac
{
    public static class AutofacContainerBuilderExtensions
    {
        private const string RequestKey = "handler";

        private const string AsyncRequestKey = "async-handler";

        public static void AddMediatR(this ContainerBuilder builder, Assembly assembly)
        {
            Decorate(builder, assembly);
        }

        private static void RegisterRequestWithResponseDecorator(ContainerBuilder builder, Type decoratorType)
        {
            builder.RegisterGenericDecorator(decoratorType, typeof(IRequestHandler<,>), fromKey: RequestKey);
        }

        private static void RegisterRequestDecorator(ContainerBuilder builder, Type decoratorType)
        {
            builder.RegisterGenericDecorator(decoratorType, typeof(IRequestHandler<>), fromKey: RequestKey);
        }

        private static void RegisterRequestHandlersFromAssembly(ContainerBuilder builder, Assembly assembly)
        {
            builder.RegisterAssemblyTypes(assembly).As(t => t.GetTypeInfo().GetInterfaces()
                .Where(i => i.IsClosedTypeOf(typeof(IRequestHandler<,>)) || i.IsClosedTypeOf(typeof(IRequestHandler<>))).Select(i => new KeyedService(RequestKey, i)));
        }
        
        private static void RegisterNotificationHandlersFromAssembly(ContainerBuilder builder, Assembly assembly)
        {
            builder.RegisterAssemblyTypes(assembly)
                .As(t => t.GetTypeInfo().GetInterfaces().Where(i => i.IsClosedTypeOf(typeof(INotificationHandler<>))).ToArray());
        }
        
        public static void Decorate(ContainerBuilder builder, Assembly handlersAssembly)
        {
            builder.RegisterSource(new ContravariantRegistrationSource());
            builder.RegisterAssemblyTypes(typeof(IMediator).GetTypeInfo().Assembly).AsImplementedInterfaces();
            builder.Register<SingleInstanceFactory>(ctx =>
            {
                var c = ctx.Resolve<IComponentContext>();
                return t => c.Resolve(t);
            });
            builder.Register<MultiInstanceFactory>(ctx =>
            {
                var c = ctx.Resolve<IComponentContext>();
                return t => (IEnumerable<object>)c.Resolve(typeof(IEnumerable<>).MakeGenericType(t));
            });

            RegisterRequestHandlersFromAssembly(builder, handlersAssembly);
            RegisterNotificationHandlersFromAssembly(builder, handlersAssembly);
            RegisterRequestWithResponseDecorator(builder, typeof(RequestHandlerWrapper<,>));
            RegisterRequestDecorator(builder, typeof(RequestHandlerWrapper<>));
        }
    }
}
