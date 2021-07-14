using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;

namespace GuimoSoft.Bus.Kafka.Consumer
{
    internal static class Logger
    {
        internal static void LogError(string message, Exception exception)
        {
            ExpandoObject dicionarioDeLog = new ExpandoObject();
            dicionarioDeLog .TryAdd(Constants.KEY_MESSAGE, message);
            if (exception is not null)
            {
                dicionarioDeLog.TryAdd(Constants.KEY_ERROR_MESSAGE, exception.Message);
                dicionarioDeLog.TryAdd(Constants.KEY_ERROR_TYPE, exception.GetType().Name);
                dicionarioDeLog.TryAdd(Constants.KEY_STACK_TRACE, exception.StackTrace);
            }
            dicionarioDeLog .TryAdd(Constants.KEY_SEVERITY, Constants.SEVERIDADE_ERROR_STRING);
            
            Console.Error.WriteLine(JsonSerializer.Serialize(dicionarioDeLog));
        }

        internal static void LogWarning(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                ExpandoObject dicionarioDeLog  = new ExpandoObject();
                dicionarioDeLog.TryAdd(Constants.KEY_MESSAGE, message);
                dicionarioDeLog.TryAdd(Constants.KEY_SEVERITY, Constants.SEVERIDADE_ERROR_STRING);
                Console.WriteLine(JsonSerializer.Serialize(dicionarioDeLog));
            }
        }
    }
}
