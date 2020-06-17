using LiteLoader.Pooling;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LiteLoader.DependencyInjection
{
    class LiteLoaderBinder : Binder
    {
        public override FieldInfo BindToField(BindingFlags bindingAttr, FieldInfo[] match, object value, CultureInfo culture)
        {
            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }
            
            for (int i = 0; i < match.Length; i++)
            {
                FieldInfo _ = match[i];

                if (CanChangeType(value, _.FieldType, culture))
                {
                    return _;
                }
            }

            return null;
        }

        public override MethodBase BindToMethod(BindingFlags bindingAttr, MethodBase[] match, ref object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] names, out object state)
        {
            
            throw new NotImplementedException();
        }

        public bool CanChangeType(object value, Type type, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ChangeType(object value, Type type, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override void ReorderArgumentArray(ref object[] args, object state)
        {
            throw new NotImplementedException();
        }

        public override MethodBase SelectMethod(BindingFlags bindingAttr, MethodBase[] match, Type[] types, ParameterModifier[] modifiers)
        {
            throw new NotImplementedException();
        }

        public override PropertyInfo SelectProperty(BindingFlags bindingAttr, PropertyInfo[] match, Type returnType, Type[] indexes, ParameterModifier[] modifiers)
        {
            throw new NotImplementedException();
        }
    }
}
