using System;
using System.Linq;
using System.Reflection;

namespace LiteLoader.Plugins
{
    public abstract class Plugin : IPlugin
    {
        #region Information

        /// <inheritdoc cref="MemberInfo.Name"/>
        public string Name { get; }

        /// <inheritdoc cref="PluginInfoAttribute.Title"/>
        public string Title { get; }

        /// <inheritdoc cref="PluginInfoAttribute.Author"/>
        public string Author { get; }

        /// <inheritdoc cref="PluginInfoAttribute.Version"/>
        public Version Version { get; }

        #endregion

        protected Plugin()
        {
            PluginInfoAttribute info = GetType()
                .GetCustomAttributes(typeof(PluginInfoAttribute), false)
                .FirstOrDefault() as PluginInfoAttribute;

            if (info == null)
            {
                throw new NullReferenceException("Missing PluginInfoAttribute");
            }

            Name = GetType().Name;
            Title = info.Title;
            Author = info.Author;
            Version = info.Version;
        }

        #region Overloads

        public int CompareTo(IPlugin other)
        {
            if (other == null)
            {
                return 1;
            }

            Type met = GetType();
            Type friendt = other.GetType();
            string me = GetType().Name;
            string friend = other.GetType().Name;

            int con = me.CompareTo(friend);

            if (con == 0)
            {
                return met.FullName.CompareTo(friendt.FullName);
            }

            return con;
        }

        public bool Equals(IPlugin other)
        {
            if (other == null)
            {
                return false;
            }

            Type type = GetType();
            Type tther = other.GetType();

            return type.Equals(tther);
        }

        #endregion
    }
}
