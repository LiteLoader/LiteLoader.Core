using LiteLoader.DependencyInjection;
using LiteLoader.Pooling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace LiteLoader.Modules
{
    internal sealed class DotNetModuleLoader : IModuleLoader
    {
        private Queue<ModuleInfo> _loadingModules { get; }
        private List<Module> _loadedModules { get; }
        private List<ModuleInfo> _unloadedModules { get; }

        public IEnumerable<string> LoadedModules { get; }

        public IEnumerable<string> UnloadedModules => throw new NotImplementedException();

        public DotNetModuleLoader()
        {
            _loadingModules = new Queue<ModuleInfo>();
            _loadedModules = new List<Module>();
            _unloadedModules = new List<ModuleInfo>();
        }



        private void FinalizeModuleLoad(Module module)
        {
            lock (_loadedModules)
            {
                if (!_loadedModules.Contains(module))
                {
                    _loadedModules.Add(module);
                    _loadedModules.Sort(ModuleComparer.Instance);
                }
            }
        }

        public IModule GetModule(string moduleName)
        {
            if (string.IsNullOrEmpty(moduleName))
            {
                return null;
            }

            lock (_loadedModules)
            {
                for (int i = 0; i < _loadedModules.Count; i++)
                {
                    Module module = _loadedModules[i];
                    Type t = module.GetType();
                    
                    if (t.Name.Equals(moduleName, StringComparison.OrdinalIgnoreCase))
                    {
                        return module;
                    }
                }
            }

            return null;
        }

        public bool IsModuleLoaded(string moduleName)
        {
            if (string.IsNullOrEmpty(moduleName))
            {
                return false;
            }

            lock (_loadedModules)
            {
                for (int i = 0; i < _loadedModules.Count; i++)
                {
                    Module module = _loadedModules[i];
                    string name = module.GetType().Name;

                    if (name.Equals(moduleName, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool IsModuleLoading(string moduleName)
        {
            if (string.IsNullOrEmpty(moduleName))
            {
                return false;
            }

            bool found = false;
            lock (_loadingModules)
            {
                for (int i = 0; i < _loadingModules.Count; i++)
                {
                    ModuleInfo m = _loadingModules.Dequeue();
                    
                    if (found)
                    {
                        _loadingModules.Enqueue(m);
                        continue;
                    }

                    if (m.Name.Equals(moduleName, StringComparison.OrdinalIgnoreCase))
                    {
                        found = true;
                    }

                    _loadingModules.Enqueue(m);
                }
            }

            return found;
        }

        public void LoadModule(string moduleName, bool immediately = false)
        {
            if (string.IsNullOrEmpty(moduleName))
            {
                return;
            }

            if (IsModuleLoaded(moduleName))
            {
                return;
            }

            if (IsModuleLoading(moduleName))
            {
                return;
            }

            string file = Path.Combine(Interface.CoreModule.ModuleDirectory, $"{moduleName}.dll");
            
            if (!File.Exists(file))
            {
                file = Path.Combine(Interface.CoreModule.ModuleDirectory, moduleName);
                file = Path.Combine(file, $"{moduleName}.dll");

                if (!File.Exists(file))
                {
                    return;
                }
            }

            ModuleInfo moduleInfo = new ModuleInfo(file);
            lock (_loadingModules)
            {
                if (!_loadingModules.Contains(moduleInfo))
                {
                    _loadingModules.Enqueue(moduleInfo);
                }
            }

            if (immediately)
            {
                LoadModule(moduleInfo);
            }
            else
            {
                if (!ThreadPool.QueueUserWorkItem(LoadModule, moduleInfo))
                {
                    LoadModule(moduleInfo);
                }
            }
        }

        public void UnloadModule(string moduleName)
        {
            if (string.IsNullOrEmpty(moduleName))
            {
                return;
            }

            Module module = null;
            lock (_loadedModules)
            {
                for (int i = 0; i < _loadedModules.Count; i++)
                {
                    module = _loadedModules[i];

                    if (!module.GetType().Name.Equals(moduleName, StringComparison.OrdinalIgnoreCase))
                    {
                        module = null;
                        continue;
                    }

                    _loadedModules.Remove(module);
                    break;
                }
            }

            lock (_unloadedModules)
            {
                if (!_unloadedModules.Contains(module.ModuleInfo))
                {
                    _unloadedModules.Add(module.ModuleInfo);
                }
            }

            module.ExecuteHook("Unload", Pool.Array<object>(0));
        }

        private void LoadModule(object obj)
        {
            ModuleInfo moduleInfo = (ModuleInfo)obj;
            moduleInfo.EnsureStructure();
            moduleInfo.CreateShadowFile();
            moduleInfo.LoadShadowFile();

            Module module = (Module)ActivationUtility.CreateFactory(moduleInfo.ModuleType, this).Invoke(Interface.CoreModule.ServiceProvider);
            module.ModuleInfo = moduleInfo;

            lock (_loadingModules)
            {

            }
        }
    }
}
