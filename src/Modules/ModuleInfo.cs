using LiteLoader.Logging;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LiteLoader.Modules
{
    internal sealed class ModuleInfo
    {
        public string Name { get; }

        public string ParentFile { get; private set; }

        public string ShadowFile { get; private set; }

        public string ModuleDirectory { get; }

        public string ConfigurationDirectory { get; }

        public string LocaleDirectory { get; }

        public string DataDirectory { get; }

        public Assembly Assembly { get; private set; }

        public Type ModuleType { get; private set; }

        public ModuleInfo(string moduleFile)
        {
            ParentFile = moduleFile;
            Name = Path.GetFileNameWithoutExtension(Name);
            ModuleDirectory = Path.Combine(Interface.CoreModule.ModuleDirectory, Name);
            ConfigurationDirectory = Path.Combine(ModuleDirectory, "Configuration");
            LocaleDirectory = Path.Combine(ModuleDirectory, "Locale");
            DataDirectory = Path.Combine(ModuleDirectory, "Data");
        }

        public void EnsureStructure()
        {
            if (!Directory.Exists(ModuleDirectory))
            {
                Interface.CoreModule.RootLogger.Debug("Creating module directory for {0}", Name);
                Directory.CreateDirectory(ModuleDirectory);
            }

            if (!Directory.Exists(ConfigurationDirectory))
            {
                Interface.CoreModule.RootLogger.Debug("Creating module configuration directory for {0}", Name);
                Directory.CreateDirectory(ConfigurationDirectory);
            }

            if (!Directory.Exists(LocaleDirectory))
            {
                Interface.CoreModule.RootLogger.Debug("Creating module locale directory for {0}", Name);
                Directory.CreateDirectory(LocaleDirectory);
            }

            if (!Directory.Exists(DataDirectory))
            {
                Interface.CoreModule.RootLogger.Debug("Creating module data directory for {0}", Name);
                Directory.CreateDirectory(DataDirectory);
            }

            if (!ParentFile.StartsWith(ModuleDirectory, StringComparison.OrdinalIgnoreCase))
            {
                Interface.CoreModule.RootLogger.Debug("Moving module {0} inside parent folder", Name);
                string newFile = Path.Combine(ModuleDirectory, Path.GetFileName(ParentFile));
                File.Move(ParentFile, newFile);
                ParentFile = newFile;
            }
        }

        public void CreateShadowFile()
        {
            if (ShadowFile != null)
            {
                return;
            }

            ShadowFile = Path.Combine(Interface.CoreModule.TemporaryDirectory, "ModuleCache");
            
            if (!Directory.Exists(ShadowFile))
            {
                Directory.CreateDirectory(ShadowFile);
            }

            ShadowFile = Path.Combine(ShadowFile, Guid.NewGuid().ToString("B") + ".dll");
            File.Copy(ParentFile, ShadowFile);
            Interface.CoreModule.RootLogger.Debug("Created shadow file of module {0}", Name);
        }

        public void LoadShadowFile()
        {
            if (string.IsNullOrEmpty(ShadowFile))
            {
                return;
            }

            if (Assembly != null)
            {
                return;
            }

            Assembly = Assembly.LoadFrom(ShadowFile);
            Interface.CoreModule.RootLogger.Debug("Loaded assembly for module {0}", Name);
            ModuleType = Assembly.GetExportedTypes()
                .FirstOrDefault(t => typeof(Module).IsAssignableFrom(t) && !t.IsAbstract);

            if (ModuleType == null)
            {
                Interface.CoreModule.RootLogger.Error("Unable to locate Module class for assembly {0}", Assembly.FullName);
            }
        }
    }
}
