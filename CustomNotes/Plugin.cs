using System.Reflection;
using CustomNotes.Installers;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using SiraUtil.Zenject;
using static System.DateTime;
using static IPA.Utilities.Utils;
using IPALogger = IPA.Logging.Logger;

namespace CustomNotes;

[Plugin(RuntimeOptions.DynamicInit), NoEnableDisable]
public class Plugin
{
    public static IPALogger Log { get; private set; } = null!;
    public static PluginConfig Config { get; private set; } = null!;
    public static Assembly ExecutingAssembly { get; } = Assembly.GetExecutingAssembly();

    public static bool IsAprilFirst => (CanUseDateTimeNowSafely ? Now : UtcNow) is { Month: 4, Day: 1 };
        
    [Init]
    public Plugin(IPALogger logger, Config config, Zenjector zenjector)
    {
        Log = logger;
        Config = config.Generated<PluginConfig>();
            
        zenjector.Install<AppInstaller>(Location.App, Config);
        zenjector.Install<MenuInstaller>(Location.Menu);
        zenjector.Install<PlayerInstaller>(Location.Player);
    }
}