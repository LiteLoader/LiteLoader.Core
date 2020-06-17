using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LiteLoader.DependencyInjection
{
    internal class ExecutionEngine : IExecutionEngine
    {
        public object ExecuteMethod(MethodBase method, object[] arguments, object instance = null, IServiceProvider serviceProvider = null)
        {
            throw new NotImplementedException();
        }

        private object ExecuteMethod(ConstructorInfo constructor, object[] arguments)
        {
            return constructor.Invoke(arguments);
        }
    }
}
