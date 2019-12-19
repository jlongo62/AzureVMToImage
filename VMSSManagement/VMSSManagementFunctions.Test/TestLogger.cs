using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace VMSSManagementFunctions.Test
{
    public class TestLogger : ILogger
    {
        public IList<string> Logs;

        public TestContext TestContext { get; private set; }

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => false;

        public TestLogger(TestContext testContext)
        {
            this.TestContext = testContext;
        }

        public void Log<TState>(LogLevel logLevel,
                                EventId eventId,
                                TState state,
                                Exception exception,
                                Func<TState, Exception, string> formatter)
        {
            string message = formatter(state, exception);
            TestContext.WriteLine(message);
        }
    }
}
