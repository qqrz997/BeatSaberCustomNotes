using CustomNotes.Managers;
using Zenject;

namespace CustomNotes.Installers;

internal class AppInstaller : Installer
{
    private readonly PluginConfig config;

    private AppInstaller(PluginConfig config)
    {
        this.config = config;
    }
        
    public override void InstallBindings()
    {
        Container.BindInstance(config).AsSingle();
        
        Container.Bind<NoteFileManager>().AsSingle();
        Container.BindInterfacesTo<NoteDataLoader>().AsSingle();
        Container.Bind<NotesLoader>().AsSingle();
        Container.Bind<Note2Loader>().AsSingle();
        Container.Bind<LegacyCustomNoteLoader>().AsSingle();
        
        Container.Bind<GameResourcesManager>().AsSingle();
    }
}