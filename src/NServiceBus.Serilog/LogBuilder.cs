﻿using System.Collections.Concurrent;
using Serilog;
using Serilog.Core;
using Serilog.Core.Enrichers;

class LogBuilder
{
    ConcurrentDictionary<string, ILogger> loggers = new();

    public LogBuilder(ILogger logger, string endpointName)
    {
        Logger = logger
            .ForContext(new[]
            {
                new PropertyEnricher("ProcessingEndpoint", endpointName)
            });
    }

    public ILogger Logger { get; }

    public ILogger GetLogger(string key)
    {
        return loggers.GetOrAdd(key, x => Logger
            .ForContext(Constants.SourceContextPropertyName, x));
    }
}