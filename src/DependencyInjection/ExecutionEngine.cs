using LiteLoader.Pooling;
using System;
using System.ComponentModel;
using System.Reflection;

namespace LiteLoader.DependencyInjection
{
    internal class ExecutionEngine : IExecutionEngine
    {
        public object ExecuteMethod(MethodBase method, object[] arguments, object instance = null, IServiceProvider serviceProvider = null)
        {
            if (arguments == null)
            {
                arguments = Pool.Array<object>(0);
            }

            ParameterInfo[] parameters = method.GetParameters();
            object[] argvars = CreateParameterMap(parameters, arguments, out int?[] map, serviceProvider);

            if (argvars == null)
            {
                throw new ExecutionEngineException("Unable to execute method with provided arguments");
            }

            object value;
            try
            {
                if (method is ConstructorInfo ctor)
                {
                    value = ctor.Invoke(argvars);
                }
                else
                {
                    value = method.Invoke(instance, argvars);
                }
                HandleByRefs(parameters, map, argvars, arguments);
                return value;
            }
            finally
            {
                Pool.Free(argvars);
                Pool.Free(map);
            }
        }

        private object[] CreateParameterMap(ParameterInfo[] parameters, object[] arguments, out int?[] map, IServiceProvider serviceProvider = null)
        {
            map = Pool.Array<int?>(parameters.Length);
            object[] argVars = Pool.Array<object>(parameters.Length);

            int argSkip = 0;
            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo p = parameters[i];
                Type pt = p.ParameterType;

                if (pt.IsByRef)
                {
                    pt = pt.GetElementType();
                }

                bool found = false;
                for (int n = 0 + argSkip; n < arguments.Length; n++)
                {
                    object a = arguments[n];
                    
                    if (a == null)
                    {
                        if (p.IsOut || pt.IsByRef)
                        {
                            argSkip++;
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
                        TypeConverter converter = TypeDescriptor.GetConverter(pt);
                        if (converter.CanConvertFrom(at))
                        {
                            argSkip++;
                            map[p.Position] = n;
                            found = true;
                            argVars[p.Position] = converter.ConvertFrom(a);
                        }
                        else
                        {
                            converter = TypeDescriptor.GetConverter(at);
                            if (converter.CanConvertTo(pt))
                            {
                                argSkip++;
                                map[p.Position] = n;
                                found = true;
                                argVars[p.Position] = converter.ConvertTo(a, pt);
                            }
                        }

                        break;
                    }

                    argSkip++;
                    map[p.Position] = n;
                    found = true;
                    argVars[p.Position] = a;
                    break;
                }
                
                if (!found)
                {
                    object service = serviceProvider?.GetService(pt);
                    if (service == null)
                    {
                        Pool.Free(argVars);
                        Pool.Free(map);
                        map = null;
                        argVars = null;
                        return argVars;
                    }
                    argVars[p.Position] = service;
                }
            }

            return argVars;
        }

        private void HandleByRefs(ParameterInfo[] parameters, int?[] map, object[] argVar, object[] original)
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
                    original[map[i].Value] = argVar[i];
                }
            }
        }
    }
}
