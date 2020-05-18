using LiteLoader.Pooling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LiteLoader.DependencyInjection
{
    public static class ActivationUtility
    {

        public static Func<IServiceProvider, object> CreateFactory(Type implementationType, params object[] optionalArguments)
        {
            if (implementationType == null)
            {
                throw new ArgumentNullException(nameof(implementationType));
            }

            if (implementationType.IsAbstract)
            {
                throw new ArgumentException("Type can't be abstract", nameof(implementationType));
            }

            ConstructorInfo[] constructors = implementationType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            Type[] parameters = Pool.Array<Type>(0);

            if (optionalArguments != null && optionalArguments.Length > 0)
            {
                Pool.Free(parameters);
                parameters = Pool.Array<Type>(optionalArguments.Length);

                for (int i = 0; i < optionalArguments.Length; i++)
                {
                    parameters[i] = optionalArguments[i].GetType();
                }
            }

            ConstructorInfo constructor = FilterConstructors(constructors, parameters);
            Pool.Free(parameters);

            if (constructor == null)
            {
                throw new MissingMemberException($"Unable to find suitable constructor for {implementationType.FullName}");
            }

            ParameterInfo[] parameterTypes = constructor.GetParameters();

            return new Func<IServiceProvider, object>(provider =>
            {
                object[] arguments = Pool.Array<object>(parameterTypes.Length);

                AssignParameters(arguments, parameterTypes, optionalArguments, provider);

                object instance = constructor.Invoke(arguments);
                Pool.Free(arguments);
                return instance;
            });
        }

        private static ConstructorInfo FilterConstructors(ConstructorInfo[] constructors, Type[] parameterTypes)
        {
            IEnumerable<ConstructorInfo> filtered = constructors
                .Where(c => HasMatchingParameters(c.GetParameters(), parameterTypes))
                .OrderByDescending(c => c.GetParameters().Length).ToArray();

            ConstructorInfo attribute = filtered.FirstOrDefault(c => HasFactoryAttribute(c));

            if (attribute != null)
            {
                return attribute;
            }

            return filtered.FirstOrDefault();
        }

        private static bool HasFactoryAttribute(ConstructorInfo constructor)
        {
            return constructor.GetCustomAttributes(typeof(FactoryAttribute), false).FirstOrDefault() != null;
        }

        private static bool IsMatchingParameter(ParameterInfo parameter, Type argument)
        {
            if (argument == null)
            {
                if (parameter.IsOptional || parameter.DefaultValue != DBNull.Value)
                {
                    return true;
                }

                return false;
            }

            if (!parameter.ParameterType.IsAssignableFrom(argument))
            {
                return false;
            }

            return true;
        }

        private static bool HasMatchingParameters(ParameterInfo[] parameters, Type[] arguments, int startIndex = 0)
        {
            if ((parameters.Length - startIndex) < arguments.Length)
            {
                return false;
            }

            for (int i = 0; i < arguments.Length; i++)
            {
                if (!IsMatchingParameter(parameters[startIndex + i], arguments[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static void AssignParameters(object[] values, ParameterInfo[] parameters, object[] provided, IServiceProvider provider)
        {
            int pi = 0;
            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo p = parameters[i];
                object value = null;
                if (pi < provided.Length)
                {
                    value = provided[pi];
                    pi++;
                }
                else
                {
                    value = provider.GetService(p.ParameterType);
                }

                if (value == null)
                {
                    if (p.IsOptional || p.DefaultValue != DBNull.Value)
                    {
                        if (p.DefaultValue != DBNull.Value)
                        {
                            value = p.DefaultValue;
                        }
                        else
                        {
                            if (!CanAssignNull(p.ParameterType))
                            {
                                value = Activator.CreateInstance(p.ParameterType);
                            }
                        }
                    }
                    else
                    {
                        throw new ArgumentNullException(p.Name, $"Missing required parameter for {p.Member.ToString()}");
                    }
                }
                else
                {
                    if (!p.ParameterType.IsInstanceOfType(value))
                    {
                        throw new ArgumentException($"Incorrect value assignment {p.ParameterType} != {value.GetType()}", p.Name);
                    }
                }

                values[i] = value;
            }
        }

        private static bool CanAssignNull(Type type)
        {
            if (!type.IsValueType)
            {
                return true;
            }

            return Nullable.GetUnderlyingType(type) != null;
        }
    }
}
