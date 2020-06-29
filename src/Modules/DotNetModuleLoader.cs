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
        private sealed class ModuleReference
        {
            public Assembly Assembly { get; internal set; }

            public Type ModuleType { get; internal set; }

            public Func<IServiceProvider, object> Factory { get; internal set; }

            public IModule Module { get; internal set; }

            public string ModulePath { get; }

            public string ShadowPath { get; }

            public bool IsLoaded { get; internal set; }

            public bool IsLoading { get; internal set; }

            public ModuleReference(string modulePath, string shadowPath)
            {
                ModulePath = modulePath;
                ShadowPath = shadowPath;
                IsLoaded = false;
                IsLoading = false;
            }
        }

        private readonly List<ModuleReference> moduleReferences = new List<ModuleReference>();

        public IEnumerable<string> LoadedModules => throw new NotImplementedException();

        public IEnumerable<string> UnloadedModules => throw new NotImplementedException();

        public IModule GetModule(string moduleName)
        {
            throw new NotImplementedException();
        }

        public bool IsModuleLoaded(string moduleName)
        {
            throw new NotImplementedException();
        }

        public bool IsModuleLoading(string moduleName)
        {
            throw new NotImplementedException();
        }

        public void LoadModule(string moduleName, bool immediately = false)
        {
            if (string.IsNullOrEmpty(moduleName))
            {
                return;
            }

            string path = Path.Combine(Interface.CoreModule.ModuleDirectory, $"{moduleName}.dll");

            if (!File.Exists(path))
            {
                return;
            }

            ModuleReference reference = new ModuleReference(Path.GetFileName(path), Path.GetTempFileName());
            lock (moduleReferences)
            {
                moduleReferences.Add(reference);
            }

            reference.IsLoading = true;

            if (immediately)
            {
                ProcessAsyncronousRawLoad(reference);
            }
            else
            {
                ThreadPool.QueueUserWorkItem(ProcessAsyncronousRawLoad, reference);
            }
        }

        public void UnloadModule(string moduleName)
        {
            throw new NotImplementedException();
        }

        private void ProcessAsyncronousRawLoad(object obj)
        {
            if (obj == null || !(obj is ModuleReference reference))
            {
                return;
            }

            FileInfo original = new FileInfo(reference.ModulePath);
            FileInfo temp = new FileInfo(reference.ShadowPath);
            
            try
            {
                temp = original.CopyToTempPath(temp);
            }
            catch (Exception)
            {
                // TODO: Handle Fail
                return;
            }

            try
            {
                reference.Assembly = Assembly.LoadFrom(temp.FullName);
            }
            catch (Exception)
            {
                // TODO: Handle Fail
                return;
            }

            reference.ModuleType = reference.Assembly
                .GetExportedTypes()
                .FirstOrDefault(t => typeof(IModule).IsAssignableFrom(t) && !t.IsAbstract);
        
            if (reference.ModuleType == null)
            {
                // TODO: Handle Fail
                return;
            }

            reference.Factory = ActivationUtility.CreateFactory(reference.ModuleType, this);
            reference.Module = (IModule)reference.Factory(Interface.CoreModule.ServiceProvider);
            reference.Module.ExecuteHook("Init", Pool.Array<object>(0));
        }
    }
}
