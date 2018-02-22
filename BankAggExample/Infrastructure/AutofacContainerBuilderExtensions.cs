using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.Scanning;
using Autofac.Features.Variance;
using BankAggExample.Infrastructure;
using MediatR;

namespace Autofac
{
    public static class AutofacContainerBuilderExtensions
    {
        private const string RequestKey = "handler";

        private const string AsyncRequestKey = "async-handler";

        public static ContainerBuilder AddMediatR(this ContainerBuilder builder, Assembly assembly)
        {
            Decorate(builder, assembly);
            return builder;
        }

        private static void RegisterRequestWithResponseDecorator(ContainerBuilder builder, Type decoratorType)
        {
            builder.RegisterGenericDecorator(decoratorType, typeof(IRequestHandler<,>), fromKey: RequestKey);
        }

        private static void RegisterRequestDecorator(ContainerBuilder builder, Type decoratorType)
        {
            builder.RegisterGenericDecorator(decoratorType, typeof(IRequestHandler<>), fromKey: RequestKey);
        }

        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> RegisterRequestHandlers(this ContainerBuilder builder, Assembly assembly)
        {
            var registration = RegisterRequestHandlersFromAssembly(builder, assembly);

            RegisterRequestWithResponseDecorator(builder, typeof(RequestHandlerWrapper<,>));
            RegisterRequestDecorator(builder, typeof(RequestHandlerWrapper<>));

            return registration;
        }

        private static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> RegisterRequestHandlersFromAssembly(ContainerBuilder builder, Assembly assembly)
        {
            return builder.RegisterAssemblyTypes(assembly).As(t => t.GetTypeInfo().GetInterfaces()
                .Where(i => i.IsClosedTypeOf(typeof(IRequestHandler<,>)) || i.IsClosedTypeOf(typeof(IRequestHandler<>)))
                .Select(i => new KeyedService(RequestKey, i)));
        }

        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> RegisterNotificationHandlers(this ContainerBuilder builder, Assembly assembly)
        {
            var registration = RegisterNotificationHandlersFromAssembly(builder, assembly);
            return registration;
        }

        private static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> RegisterNotificationHandlersFromAssembly(ContainerBuilder builder, Assembly assembly)
        {
            return builder.RegisterAssemblyTypes(assembly)
                .As(t => t.GetTypeInfo().GetInterfaces().Where(i => i.IsClosedTypeOf(typeof(INotificationHandler<>))).ToArray());
        }
        
        public static void Decorate(ContainerBuilder builder, Assembly handlersAssembly)
        {
            //builder.RegisterSource(new ContravariantRegistrationSource());
            builder.RegisterAssemblyTypes(typeof(IMediator).GetTypeInfo().Assembly).AsImplementedInterfaces();
            builder.Register<SingleInstanceFactory>(ctx =>
            {
                var c = ctx.Resolve<IComponentContext>();
                return t =>
                {
                    var instance = c.Resolve(t);
                    return instance;
                };
            });
            builder.Register<MultiInstanceFactory>(ctx =>
            {
                var c = ctx.Resolve<IComponentContext>();
                return t =>
                {
                    var returnVal = (IEnumerable<object>)c.Resolve(typeof(IEnumerable<>).MakeGenericType(t));
                    return returnVal;
                };
            });
            
        }
    }
}
