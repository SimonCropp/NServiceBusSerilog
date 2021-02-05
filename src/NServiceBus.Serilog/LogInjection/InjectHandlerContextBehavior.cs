﻿using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Pipeline;

class InjectHandlerContextBehavior :
    Behavior<IInvokeHandlerContext>
{
    public class Registration :
        RegisterStep
    {
        public Registration() :
            base(
                stepId: $"Serilog{nameof(InjectHandlerContextBehavior)}",
                behavior: typeof(InjectHandlerContextBehavior),
                description: "Injects logger into the handler context")
        {
        }
    }

    public override async Task Invoke(IInvokeHandlerContext context, Func<Task> next)
    {
        var handler = context.HandlerType();
        var bag = context.Extensions;
        var exceptionLogState = bag.Get<ExceptionLogState>();
        exceptionLogState.IncomingMessage = context.MessageBeingHandled;
        var forContext = context.Logger().ForContext("Handler", handler);
        try
        {
            bag.Set("SerilogHandlerLogger", forContext);
            await next();
        }
        finally
        {
            bag.Remove("SerilogHandlerLogger");
        }
    }
}