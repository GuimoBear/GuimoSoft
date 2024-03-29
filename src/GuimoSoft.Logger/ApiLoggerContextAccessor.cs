﻿using System.Collections.Generic;
using System.Threading;

namespace GuimoSoft.Logger
{
    internal class ApiLoggerContextAccessor : IApiLoggerContextAccessor
    {
        private static readonly AsyncLocal<ApiLogerContextHolder> _apiLoggercontextCurrent = new AsyncLocal<ApiLogerContextHolder>();

        public IDictionary<string, object> Context
        {
            get
            {
                return _apiLoggercontextCurrent.Value?.Context;
            }
            set
            {
                var holder = _apiLoggercontextCurrent.Value;
                if (holder != null)
                    holder.Context = null;

                if (value != null)
                    _apiLoggercontextCurrent.Value = new ApiLogerContextHolder { Context = value };
            }
        }

        private sealed class ApiLogerContextHolder
        {
            public IDictionary<string, object> Context;
        }
    }
}
