﻿using GuimoSoft.Bus.Abstractions;
using System.Collections.Generic;

namespace GuimoSoft.Bus.Tests.Fakes
{
    public class FakePipelineMessage : IMessage
    {
        public string LastMiddlewareToRun { get; }
        public List<string> MiddlewareNames { get; } = new List<string>();

        public FakePipelineMessage()
        {

        }

        public FakePipelineMessage(string lastMiddlewareToRun)
        {
            LastMiddlewareToRun = lastMiddlewareToRun;
        }
    }
}
