using LiteLoader.Pooling;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

#if !NET35
using System.Threading.Tasks;
#endif

namespace LiteLoader.DependencyInjection
{
    internal class ExecutionEngine : IExecutionEngine
    {
        public object ExecuteMethod(MethodBase method, object[] arguments, object instance = null)
        {
            if (arguments == null)
            {
                arguments = Pool.Array<object>(0);
            }

            ParameterInfo[] parameters = method.GetParameters();
            object value;
            if (method is ConstructorInfo ctor)
            {
                value = ctor.Invoke(arguments);
            }
            else
            {
                value = method.Invoke(instance, arguments);
            }

#if !NET35

            if (value != null && typeof(Task).IsInstanceOfType(value))
            {
                Task task = (Task)value;
                value = task.AwaitResult(token: Interface.CoreModule.GenerateCancellationToken());
            }
#endif

            return value;
        }

        public bool CreateParameterMap(ParameterInfo[] parameters, object[] arguments, out int?[] map, out object[] newArgs, out int usedArguments, IServiceProvider serviceProvider = null)
        {
            map = null;
            newArgs = null;
            usedArguments = 0;
            if (parameters == null)
            {
                return false;
            }

            if (parameters.Length == 0)
            {
                map = Pool.Array<int?>(0);
                newArgs = Pool.Array<object>(0);
                return true;
            }

            map = Pool.Array<int?>(parameters.Length);
            newArgs = Pool.Array<object>(parameters.Length);

            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo p = parameters[i];
                Type pt = p.ParameterType;

                if (pt.IsByRef)
                {
                    pt = pt.GetElementType();
                }

                bool found = false;
                for (int n = 0 + usedArguments; n < arguments.Length; n++)
                {
                    object a = arguments[n];

                    if (a == null)
                    {
                        if (p.IsOut || pt.IsByRef)
                        {
                            usedArguments++;
                            map[p.Position] = n;
                            found = true;
                        }
                        break;
                    }

                    if (p.IsOut)
                    {
                        break;
                    }

                    Type at = a.GetType();
                    if (!pt.IsAssignableFrom(at))
                    {
                        if (TryConvert(a, pt, out object converted))
                        {
                            usedArguments++;
                            map[p.Position] = n;
                            found = true;
                            newArgs[p.Position] = converted;
                        }
                        break;
                    }

                    usedArguments++;
                    map[p.Position] = n;
                    found = true;
                    newArgs[p.Position] = a;
                    break;
                }

                if (!found)
                {
                    object service = serviceProvider?.GetService(pt);
                    if (service == null)
                    {
                        Pool.Free(newArgs);
                        Pool.Free(map);
                        map = null;
                        newArgs = null;
                        return false;
                    }
                    newArgs[p.Position] = service;
                }
            }

            return true;
        }

        public void ProcessByRefs(ParameterInfo[] parameters, object[] args, object[] original, int?[] map)
        {
            for (int i = 0; i < map.Length; i++)
            {
                if (!map[i].HasValue)
                {
                    continue;
                }

                ParameterInfo p = null;

                for (int n = 0; n < parameters.Length; n++)
                {
                    if (parameters[n].Position != i)
                    {
                        continue;
                    }

                    p = parameters[n];
                    break;
                }

                if (p == null)
                {
                    continue;
                }

                if (p.IsOut || p.ParameterType.IsByRef)
                {
                    object originalValue = original[map[i].Value];

                    if (originalValue != null)
                    {
                        Type oT = originalValue.GetType();

                        if (!oT.IsInstanceOfType(args[i]))
                        {
                            if (TryConvert(args[i], oT, out object converted))
                            {
                                original[map[i].Value] = converted;
                                continue;
                            }
                        }
                    }

                    original[map[i].Value] = args[i];
                }
            }
        }

        private bool TryConvert(object value, Type requestedType, out object converted)
        {
            converted = null;
            if (value == null || requestedType == null)
            {
                return false;
            }

            TypeConverter converter = TypeDescriptor.GetConverter(requestedType);
            Type valueType = value.GetType();
            if (converter != null)
            {
                if (converter.CanConvertFrom(valueType))
                {
                    converted = converter.ConvertFrom(value);
                    return true;
                }
            }

            converter = TypeDescriptor.GetConverter(valueType);
            if (converter != null)
            {
                if (converter.CanConvertTo(requestedType))
                {
                    converted = converter.ConvertTo(value, requestedType);
                    return true;
                }
            }

            return false;
        }

        public MethodBase FindBestMethod(IEnumerable<MethodBase> methods, object[] arguments, out object[] newArgs, out int?[] map, out ParameterInfo[] parameters, IServiceProvider serviceProvider = null)
        {
            MethodBase best = null;
            int usedArguments = -1;
            newArgs = null;
            map = null;
            parameters = null;
            int count = methods.Count();

            for (int i = 0; i < count; i++)
            {
                MethodBase method = methods.ElementAt(i);
                ParameterInfo[] param = method.GetParameters();

                if (!CreateParameterMap(param, arguments, out int?[] pMap, out object[] pArgs, out int usedArgs, serviceProvider))
                {
                    continue;
                }

                if (usedArgs > usedArguments)
                {
                    if (newArgs != null)
                    {
                        Pool.Free(newArgs);
                    }

                    if (map != null)
                    {
                        Pool.Free(map);
                    }

                    best = method;
                    parameters = param;
                    newArgs = pArgs;
                    map = pMap;
                }
                else
                {
                    Pool.Free(pArgs);
                    Pool.Free(pMap);
                }
            }

            return best;
        }
    }
}
