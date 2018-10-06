﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NServiceBus.Pipeline;
using Serilog;
using Serilog.Events;
using Serilog.Parsing;

class IncomingLogicalMessageBehavior : Behavior<IIncomingLogicalMessageContext>
{
    MessageTemplate messageTemplate;
    ILogger logger;

    public IncomingLogicalMessageBehavior(LogBuilder logBuilder)
    {
        var templateParser = new MessageTemplateParser();
        messageTemplate = templateParser.Parse("Receive message {MessageType} {MessageId}.");
        logger = logBuilder.GetLogger("NServiceBus.Serilog." + nameof(IncomingLogicalMessageBehavior));
    }

    public class Registration : RegisterStep
    {
        public Registration(LogBuilder logBuilder)
            : base(
                stepId: "Serilog" + nameof(IncomingLogicalMessageBehavior),
                behavior: typeof(IncomingLogicalMessageBehavior),
                description: "Logs incoming messages",
                factoryMethod: builder => new IncomingLogicalMessageBehavior(logBuilder)
                )
        {
            InsertBefore("MutateIncomingMessages");
        }
    }

    public override Task Invoke(IIncomingLogicalMessageContext context, Func<Task> next)
    {
        var message = context.Message;
        var properties = new List<LogEventProperty>
        {
            new LogEventProperty("MessageType", new ScalarValue(message.MessageType))
        };

        if (logger.BindProperty("MessageId", context.MessageId, out var messageId))
        {
            properties.Add(messageId);
        }

        if (logger.BindProperty("Message", message.Instance, out var messageProperty))
        {
            properties.Add(messageProperty);
        }

        properties.AddRange(logger.BuildHeaders(context.Headers));
        logger.WriteInfo(messageTemplate, properties);
        return next();
    }
}