using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiteLoader.Plugins
{
    internal sealed class PluginLoader : IPluginLoader
    {
        private readonly List<string> _loadingPlugins;
        private readonly List<Plugin> _loadedPlugins;

        public PluginLoader()
        {
            _loadedPlugins = new List<Plugin>();
            _loadingPlugins = new List<string>();
        }

        public IEnumerable<string> LoadedPlugins
        {
            get
            {
                lock (_loadedPlugins)
                {
                    return _loadedPlugins.Select(p => p.Name);
                }
            }
        }

        public IEnumerable<string> UnloadedPlugins => throw new NotImplementedException();

        /// <inheritdoc cref="IPluginLoader.GetPlugin(string)"/>
        public IPlugin GetPlugin(string pluginName)
        {
            if (string.IsNullOrEmpty(pluginName))
            {
                throw new ArgumentNullException(nameof(pluginName), "Plugin name requires a value");
            }

            lock (_loadedPlugins)
            {
                for (int i = 0; i < _loadedPlugins.Count; i++)
                {
                    if (_loadedPlugins[i].Name.Equals(pluginName, StringComparison.Ordinal))
                    {
                        return _loadedPlugins[i];
                    }
                }
            }

            return null;
        }

        /// <inheritdoc cref="IPluginLoader.IsPluginLoaded(string)"/>
        public bool IsPluginLoaded(string pluginName)
        {
            if (string.IsNullOrEmpty(pluginName))
            {
                throw new ArgumentNullException(nameof(pluginName), "Plugin name requires a value");
            }

            lock (_loadedPlugins)
            {
                for (int i = 0; i < _loadedPlugins.Count; i++)
                {
                    if (_loadedPlugins[i].Name.Equals(pluginName, StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <inheritdoc cref="IPluginLoader.IsPluginLoading(string)"/>
        public bool IsPluginLoading(string pluginName)
        {
            if (string.IsNullOrEmpty(pluginName))
            {
                throw new ArgumentNullException(nameof(pluginName), "Plugin name requires a value");
            }

            lock (_loadingPlugins)
            {
                for (int i = 0; i < _loadingPlugins.Count; i++)
                {
                    if (_loadingPlugins[i].Equals(pluginName, StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <inheritdoc cref="IPluginLoader.LoadPlugin(string, bool)"/>
        public void LoadPlugin(string pluginName, bool immediately = false)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc cref="IPluginLoader.UnloadPlugin(string)"/>
        public void UnloadPlugin(string pluginName)
        {
            throw new NotImplementedException();
        }
    }
}
