﻿using System.Diagnostics;
using System.IO;
using AutomaticTypeMapper;
using EndlessClient.Initialization;
using EOLib.Config;
using EOLib.Graphics;
using EOLib.Localization;

#if !LINUX
using System.Windows.Forms;
#endif

namespace EndlessClient.GameExecution
{
    public abstract class GameRunnerBase : IGameRunner
    {
        private readonly ITypeRegistry _registry;
        private readonly string[] _args;

        protected GameRunnerBase(ITypeRegistry registry, string[] args)
        {
            _registry = registry;
            _args = args;
        }

        public virtual bool SetupDependencies()
        {
            _registry.RegisterDiscoveredTypes();

            var initializers = _registry.ResolveAll<IGameInitializer>();
            try
            {
                foreach (var initializer in initializers)
                {
                    initializer.Initialize();
                }
            }
            catch (ConfigLoadException cle)
            {
                ShowErrorMessage(cle.Message, "Error loading config file!");
                return false;
            }
            catch (DataFileLoadException dfle)
            {
                ShowErrorMessage(dfle.Message, "Error loading data files!");
                return false;
            }
            catch (DirectoryNotFoundException dnfe)
            {
                ShowErrorMessage(dnfe.Message, "Missing required directory");
                return false;
            }
            catch (FileNotFoundException fnfe)
            {
                ShowErrorMessage(fnfe.Message, "Missing required file");
                return false;
            }
            catch (LibraryLoadException lle)
            {
                var message =
                    $"There was an error loading GFX{(int) lle.WhichGFX:000}.EGF : {lle.WhichGFX}. Place all .GFX files in .\\gfx\\. The error message is:\n\n\"{lle.Message}\"";
                ShowErrorMessage(message, "GFX Load Error");
                return false;
            }

            for (int i = 0; i < _args.Length; ++i)
            {
                var arg = _args[i];

                if (string.Equals(arg, "--host") && i < _args.Length - 1)
                {
                    var host = _args[i + 1];
                    _registry.Resolve<IConfigurationRepository>()
                        .Host = host;

                    i++;
                }
                else if(string.Equals(arg, "--clonecompat"))
                {
                    _registry.Resolve<IConfigurationRepository>()
                        .MainCloneCompat = true;
                }
                else if (string.Equals(arg, "--version") && i < _args.Length - 1)
                {
                    var versionStr = _args[i + 1];
                    if (!byte.TryParse(versionStr, out var version))
                    {
                        Debug.WriteLine($"Version must be a byte (0-255).");
                    }
                    else
                    {
                        _registry.Resolve<IConfigurationRepository>()
                            .VersionBuild = version;
                    }
                    
                    i++;
                }
                else
                {
                    Debug.WriteLine($"Unrecognized argument: {arg}. Will be ignored.");
                }
            }

            return true;
        }

        private void ShowErrorMessage(string message, string caption)
        {
#if !LINUX
            MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
#endif
        }

        public virtual void RunGame()
        {
            var game = _registry.Resolve<IEndlessGame>();
            game.Run();
        }
    }
}
