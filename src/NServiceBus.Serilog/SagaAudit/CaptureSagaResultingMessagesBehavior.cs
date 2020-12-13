﻿using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using NServiceBus.Pipeline;
using NServiceBus.Serilog;

class CaptureSagaResultingMessagesBehavior :
    Behavior<IOutgoingLogicalMessageContext>
{
    bool useFullTypeName;

    public CaptureSagaResultingMessagesBehavior(bool useFullTypeName)
    {
        this.useFullTypeName = useFullTypeName;
    }

    public override Task Invoke(IOutgoingLogicalMessageContext context, Func<Task> next)
    {
        AppendMessageToState(context);
        return next();
    }

    void AppendMessageToState(IOutgoingLogicalMessageContext context)
    {
        if (!context.Extensions.TryGet(out SagaUpdatedMessage sagaUpdatedMessage))
        {
            return;
        }

        var logicalMessage = context.Message;
        if (logicalMessage == null)
        {
            //this can happen on control messages
            return;
        }

        string messageType;
        if (useFullTypeName)
        {
            messageType = logicalMessage.MessageType.ToString();
        }
        else
        {
            messageType = logicalMessage.MessageType.Name;
        }

        Dictionary<string, string?> sagaResultingMessage = new()
        {
            {"Id", context.MessageId},
            {"Type", messageType},
            {"Destination", context.GetDestinationForUnicastMessages()},
            {"Intent", context.MessageIntent()}
        };
        sagaUpdatedMessage.ResultingMessages.Add(sagaResultingMessage);
    }

    public class Registration :
        RegisterStep
    {
        public Registration(bool useFullTypeName) :
            base(
                stepId: $"Serilog{nameof(CaptureSagaResultingMessagesBehavior)}",
                behavior: typeof(CaptureSagaResultingMessagesBehavior),
                description: "Reports messages outgoing from a saga to Serilog",
                factoryMethod: _ => new CaptureSagaResultingMessagesBehavior(useFullTypeName))
        {
        }
    }
}