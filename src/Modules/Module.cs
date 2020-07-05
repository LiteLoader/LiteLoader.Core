using LiteLoader.DependencyInjection;
using LiteLoader.Pooling;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace LiteLoader.Modules
{
#pragma warning disable CA1036 // Override methods on comparable types
    public abstract class Module : IModule
#pragma warning restore CA1036 // Override methods on comparable types
    {
        #region Information

        public string Author { get; }
        public string Title { get; }
        public Version Version { get; }

        internal ModuleInfo ModuleInfo { get; set; }

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

            Title = attribute.Title ?? throw new ArgumentNullException(nameof(Title), "ModuleInfoAttribute.Title is missing");
            Author = attribute.Author ?? throw new ArgumentNullException(nameof(Author), "ModuleInfoAttribute.Author is missing");
            Version = attribute.Version ?? throw new ArgumentNullException(nameof(Version), "ModuleInfoAttribute.Version is missing");
            ExecutionEngine = Interface.CoreModule.ServiceProvider.GetService<IExecutionEngine>() ?? throw new NullReferenceException("ExecutionEngine is missing");
            hookSubscriptions = new Dictionary<string, List<MethodBase>>(StringComparer.Ordinal);
        }

        #region Method Subscriptions

        private readonly Dictionary<string, List<MethodBase>> hookSubscriptions;

        public void SubscribeTo(string methodName, Type[] parameterTypes = null, Type returnType = null)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                return;
            }

            IEnumerable<MethodInfo> methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name.Equals(methodName, StringComparison.Ordinal) && m.HasMatchingSignature(parameterTypes, returnType, true));

            SubscribeTo(methods);
        }

        public void UnsubscribeFrom(string methodName, Type[] parameterTypes = null, Type returnType = null)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                return;
            }

            IEnumerable<MethodInfo> methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(_ => _.Name.Equals(methodName, StringComparison.Ordinal) && _.HasMatchingSignature(parameterTypes, returnType, true));

            UnsubscribeFrom(methods);
        }

        protected void SubscribeTo(IEnumerable<MethodInfo> methods)
        {
            if (methods == null)
            {
                return;
            }

            foreach (MethodBase @base in methods)
            {
                if (!@base.DeclaringType.IsInstanceOfType(this) || @base is ConstructorInfo)
                {
                    continue;
                }

                string name = @base.Name;
                List<MethodBase> subscriptions;
                lock (hookSubscriptions)
                {
                    if (!hookSubscriptions.TryGetValue(name, out subscriptions))
                    {
                        subscriptions = new List<MethodBase>();
                        hookSubscriptions[name] = subscriptions;
                    }
                }

                lock (subscriptions)
                {
                    if (subscriptions.Contains(@base))
                    {
                        continue;
                    }

                    subscriptions.Add(@base);
                }
            }
        }

        protected void UnsubscribeFrom(IEnumerable<MethodInfo> methods)
        {
            if (methods == null)
            {
                return;
            }

            foreach (MethodBase @base in methods)
            {
                if (!@base.DeclaringType.IsInstanceOfType(this))
                {
                    continue;
                }

                string name = @base.Name;
                List<MethodBase> subscriptions;
                lock (hookSubscriptions)
                {
                    if (!hookSubscriptions.TryGetValue(name, out subscriptions))
                    {
                        continue;
                    }
                }

                lock (subscriptions)
                {
                    if (subscriptions.Contains(@base))
                    {
                        subscriptions.Remove(@base);
                    }
                }
            }
        }

        #endregion

        public virtual object ExecuteHook(string methodName, object[] arguments)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                return null;
            }

            List<MethodBase> methods;
            lock (hookSubscriptions)
            {
                if (!hookSubscriptions.TryGetValue(methodName, out methods))
                {
                    return null;
                }
            }

            MethodBase bestMethod;
            object[] newArgs;
            int?[] map;
            ParameterInfo[] parameters;
            lock (methods)
            {
                bestMethod = ExecutionEngine.FindBestMethod(methods, arguments, out newArgs, out map, out parameters, Interface.CoreModule.ServiceProvider);
            }

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
