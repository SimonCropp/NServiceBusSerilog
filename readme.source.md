# <img src="/src/icon.png" height="30px"> NServiceBus.Serilog

[![Build status](https://ci.appveyor.com/api/projects/status/nmcughyrado8smay/branch/master?svg=true)](https://ci.appveyor.com/project/SimonCropp/nservicebus-Serilog)
[![NuGet Status](https://img.shields.io/nuget/v/NServiceBus.Serilog.svg)](https://www.nuget.org/packages/NServiceBus.Serilog/)

Add support for sending [NServiceBus](http://particular.net/NServiceBus) logging through [Serilog](http://serilog.net/)

toc

<!--- StartOpenCollectiveBackers -->

[Already a Patron? skip past this section](#endofbacking)


## Community backed

**It is expected that all developers either [become a Patron](https://opencollective.com/nservicebusextensions/contribute/patron-6976) or have a [Tidelift Subscription](#support-via-tidelift) to use NServiceBusExtensions. [Go to licensing FAQ](https://github.com/NServiceBusExtensions/Home/#licensingpatron-faq)**


### Sponsors

Support this project by [becoming a Sponsor](https://opencollective.com/nservicebusextensions/contribute/sponsor-6972). The company avatar will show up here with a website link. The avatar will also be added to all GitHub repositories under the [NServiceBusExtensions organization](https://github.com/NServiceBusExtensions).


### Patrons

Thanks to all the backing developers. Support this project by [becoming a patron](https://opencollective.com/nservicebusextensions/contribute/patron-6976).

<img src="https://opencollective.com/nservicebusextensions/tiers/patron.svg?width=890&avatarHeight=60&button=false">

<a href="#" id="endofbacking"></a>

<!--- EndOpenCollectiveBackers -->


## Support via TideLift

Support is available via a [Tidelift Subscription](https://tidelift.com/subscription/pkg/nuget-nservicebus.serilog?utm_source=nuget-nservicebus.serilog&utm_medium=referral&utm_campaign=enterprise).


## NuGet package

https://nuget.org/packages/NServiceBus.Serilog/


## Usage

snippet: SerilogInCode


## Filtering

NServiceBus can write a significant amount of information to the log. To limit this information use the filtering features of the underlying logging framework.

For example to limit log output to a specific namespace.

Here is a code configuration example for adding a [Filter](https://github.com/serilog/serilog/wiki/Configuration-Basics#filters).

snippet: SerilogFiltering


## Tracing

Writing diagnostic log entries to [Serilog](https://serilog.net/). Plugs into the low level [pipeline](https://docs.particular.net/nservicebus/pipeline) to give more detailed diagnostics.

When using Serilog for tracing, it is optional to use Serilog as the main NServiceBus logger. i.e. there is no need to include `LogManager.Use<SerilogFactory>();`.


### Create an instance of a Serilog logger

snippet: SerilogTracingLogger


### Configure the tracing feature to use that logger

snippet: SerilogTracingPassLoggerToFeature


### Contextual logger

Serilog tracing injects a contextual `Serilog.Ilogger` into the NServiceBus pipeline.

NOTE: Saga and message tracing will use the current contextual logger.

There are several layers of enrichment based on the pipeline phase.


#### Endpoint enrichment

All loggers for an endpoint will have the the property `ProcessingEndpoint` added that contains the current [endpoint name](https://docs.particular.net/nservicebus/endpoints/specify-endpoint-name).


#### Incoming message enrichment

When a message is received, the following enrichment properties are added:

 * [SourceContext](https://github.com/serilog/serilog/wiki/Writing-Log-Events#source-contexts) will be the message type [FullName](https://docs.microsoft.com/de-de/dotnet/api/system.type.fullname) extracted from the [EnclosedMessageTypes header](https://docs.particular.net/nservicebus/messaging/headers#serialization-headers-nservicebus-enclosedmessagetypes). `UnknownMessageType` will be used if no header exists. The same value will be added to a property named `MessageType`.
 * `MessageId` will be the value of the [MessageId header](https://docs.particular.net/nservicebus/messaging/headers#messaging-interaction-headers-nservicebus-messageid).
 * `CorrelationId` will be the value of the [CorrelationId header](https://docs.particular.net/nservicebus/messaging/headers#messaging-interaction-headers-nservicebus-correlationid) if it exists.
 * `ConversationId` will be the value of the [ConversationId header](https://docs.particular.net/nservicebus/messaging/headers#messaging-interaction-headers-nservicebus-conversationid) if it exists.


#### Handler enrichment

When a handler is invoked, a new logger is forked from the above enriched physical logger with a new enriched property named `Handler` that contains the the [FullName](https://docs.microsoft.com/de-de/dotnet/api/system.type.fullname) of the current handler.


#### Outgoing message enrichment

When a message is sent, the same properties as described in "Incoming message enrichment" will be added to the outgoing pipeline. Note that if a handler sends a message, the logger injected into the outgoing pipeline will be forked from the logger instance as described in "Handler enrichment". As such it will contain a property `Handler` for the handler that sent the message.


#### Accessing the logger

The contextual logger instance can be accessed from anywhere in the pipeline via `SerilogTracingExtensions.Logger(this IPipelineContext context)`.

snippet: ContextualLoggerUsage


### Exception enrichment

When an exception occurs in the message processing pipeline, the current pipeline state is added to the exception. When that exception is logged that state can be add to the log entry.

When a pipeline exception is logged, it will be enriched with the following properties:

 * `ProcessingEndpoint` will be the current [endpoint name](https://docs.particular.net/nservicebus/endpoints/specify-endpoint-name).
 * `IncomingMessageId` will be the value of the [MessageId header](https://docs.particular.net/nservicebus/messaging/headers#messaging-interaction-headers-nservicebus-messageid).
 * `IncomingTransportMessageId` will be the MessageId from the underlying [transport](https://docs.particular.net/transports/) if it exist.
 * `IncomingHeaders` will be the value of the [Message headers](https://docs.particular.net/nservicebus/messaging/headers).
 * `IncomingMessageType` will be the message type [FullName](https://docs.microsoft.com/de-de/dotnet/api/system.type.fullname) extracted from the [EnclosedMessageTypes header](https://docs.particular.net/nservicebus/messaging/headers#serialization-headers-nservicebus-enclosedmessagetypes). `UnknownMessageType` will be used if no header exists.
 * `CorrelationId` will be the value of the [CorrelationId header](https://docs.particular.net/nservicebus/messaging/headers#messaging-interaction-headers-nservicebus-correlationid) if it exists.
 * `ConversationId` will be the value of the [ConversationId header](https://docs.particular.net/nservicebus/messaging/headers#messaging-interaction-headers-nservicebus-conversationid) if it exists.
 * `HandlerType` will be type name for the current handler if it exists.
 * `IncomingMessage` will be the value of current logical message if it exists.
 * `HandlerStartTime` the UTC timestamp for when the handler started.
 * `HandlerFailureTime` the UTC timestamp for when the handler threw the exception.


### Saga tracing

snippet: EnableSagaTracing


#### Example Logs

snippet: IntegrationTests.Saga.verified.txt


### Message tracing

Both incoming and outgoing messages will be logged at the [Information level](https://github.com/serilog/serilog/wiki/Writing-Log-Events#the-role-of-the-information-level). The current message will be included in a property named `Message`. For outgoing messages any unicast routes will be included in a property named `UnicastRoutes`.

snippet: EnableMessageTracing


#### Example Logs

snippet: IntegrationTests.Handler.verified.txt


### Startup diagnostics

[Startup diagnostics](https://docs.particular.net/nservicebus/hosting/startup-diagnostics) is, in addition to its default file location, also written to Serilog with the level of `Warning`.

snippet: WriteStartupDiagnostics


## Logging to Seq

To log to [Seq](https://getseq.net/):

snippet: SerilogTracingSeq


## Sample

The sample illustrates how to customize logging by configuring Serilog targets and rules.


### Configure Serilog

snippet: ConfigureSerilog


### Pass the configuration to NServiceBus

snippet: UseConfig


### Ensure logging is flushed on shutdown

snippet: Cleanup


## Seq Sample

Illustrates customizing [Serilog](https://serilog.net/) usage to log to [Seq](https://getseq.net/).


### Prerequisites

An instance of [Seq](https://getseq.net/) running one `http://localhost:5341`.


### Configure Serilog

snippet: ConfigureSerilog


### Pass that configuration to NServiceBus

snippet: UseConfig


### Ensure logging is flushed on shutdown

snippet: Cleanup


## Security contact information

To report a security vulnerability, use the [Tidelift security contact](https://tidelift.com/security). Tidelift will coordinate the fix and disclosure.


## Icon

[Brain](https://thenounproject.com/noun/brain/#icon-No10411) designed by [Rémy Médard](https://thenounproject.com/catalarem) from [The Noun Project](https://thenounproject.com).