using LiteLoader.DependencyInjection;
using LiteLoader.Pooling;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LiteLoader.Modules
{
    public abstract class Module : IModule
    {
        #region Information

        public string Title { get; }

        public string Author { get; }

        public Version Version { get; }

        #endregion

        #region Services

        private IExecutionEngine ExecutionEngine { get; }

        #endregion

        protected Module()
        {
            ModuleInfoAttribute attribute = GetType()
                .GetCustomAttributes(typeof(ModuleInfoAttribute), false)
                .FirstOrDefault() as ModuleInfoAttribute;

            if (attribute == null)
            {
                throw new InvalidConstraintException("Missing ModuleInfoAttribute");
            }

            Title = attribute.Title;
            Author = attribute.Author;
            Version = attribute.Version;
            ExecutionEngine = Interface.CoreModule.ServiceProvider.GetService<IExecutionEngine>();
        }

        public virtual object ExecuteHook(string name, object[] arguments)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            MethodBase[] methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name.Equals(name, StringComparison.Ordinal)).ToArray();
            MethodBase bestMethod = ExecutionEngine.FindBestMethod(methods, arguments, out object[] newArgs, out int?[] map, out ParameterInfo[] parameters, Interface.CoreModule.ServiceProvider);

            if (bestMethod == null)
            {
                return null;
            }

            try
            {
                object value = ExecutionEngine.ExecuteMethod(bestMethod, newArgs, this);
                ExecutionEngine.ProcessByRefs(parameters, newArgs, arguments, map);
                return value;
            }
            finally
            {
                Pool.Free(newArgs);
                Pool.Free(map);
            }
        }

        #region Overload Members

        public int CompareTo(IModule other)
        {
            if (other == null)
            {
                return 1;
            }

            string otherName = other.GetType().Name;
            string thisName = GetType().Name;

            return thisName.CompareTo(otherName);
        }

        public bool Equals(IModule other)
        {
            if (other != null)
            {
                string otherName = other.GetType().Name;
                string thisName = GetType().Name;
                return thisName.Equals(otherName, StringComparison.Ordinal);
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is IModule module)
            {
                return Equals(module);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return GetType().Name.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Title} v{Version.ToString(3)} by {Author}";
        }

        #endregion
    }
}
