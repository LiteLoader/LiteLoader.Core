using LiteLoader.DependencyInjection;
using LiteLoader.Pooling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiteLoader
{
    public sealed class LiteLoader
    {
        #region Services

        public IDynamicServiceProvider ServiceProvider { get; }

        public IExecutionEngine Engine { get; }

        #endregion

        public LiteLoader(string gameAssembly)
        {
            ServiceProvider = new DynamicServiceProvider();
            Engine = new ExecutionEngine();
        }
    }
}
