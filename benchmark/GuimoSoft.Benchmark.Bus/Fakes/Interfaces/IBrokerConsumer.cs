﻿using System;
using System.Collections.Generic;
using System.Threading;

namespace GuimoSoft.Benchmark.Bus.Fakes.Interfaces
{
    public interface IBrokerConsumer
    {
        void CreateTopicIfNotExists(string topic);

        (string topic, byte[] @event) Consume(IEnumerable<string> topics, int millisecondsTimeout);

        (string topic, byte[] @event) Consume(IEnumerable<string> topics, CancellationToken cancellationToken);

        (string topic, byte[] @event) Consume(IEnumerable<string> topics, TimeSpan timeout);
    }
}
