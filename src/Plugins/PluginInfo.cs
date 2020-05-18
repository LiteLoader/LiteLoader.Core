using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiteLoader.Plugins
{
    internal struct PluginInfo
    {
        public string Name { get; }

        public string Title { get; }

        public string Author { get; }

        public Version Version { get; }

        public bool ForceSyncronousLoad { get; }

        public Type Type { get; }

        public PluginInfo(Type type)
        {
            Type = type;
            Name = Type.Name;

            PluginInfoAttribute attribute = type
                .GetCustomAttributes(typeof(PluginInfoAttribute), false)
                .FirstOrDefault() as PluginInfoAttribute;
            Title = attribute.Title;
            Author = attribute.Author;
            Version = attribute.Version;
            ForceSyncronousLoad = attribute.ForceSyncronousLoad;
        }
    }
}
