using System;
using Zenject;
using BeatSaberMarkupLanguage.MenuButtons;
using CustomNotes.UI;

namespace CustomNotes.Managers;

internal class MenuButtonManager : IInitializable, IDisposable
{
    private readonly MenuButton menuButton;
    private readonly MainFlowCoordinator mainFlowCoordinator;
    private readonly NotesFlowCoordinator notesFlowCoordinator;

    private MenuButtonManager(MainFlowCoordinator mainFlowCoordinator, NotesFlowCoordinator notesFlowCoordinator)
    {
        this.mainFlowCoordinator = mainFlowCoordinator;
        this.notesFlowCoordinator = notesFlowCoordinator;
        
        menuButton = new("Custom Notes", "Change Custom Notes Here!", ShowNotesFlow);
    }

    public void Initialize() => MenuButtons.Instance.RegisterButton(menuButton);
    public void Dispose() => MenuButtons.Instance.UnregisterButton(menuButton);
    private void ShowNotesFlow() => mainFlowCoordinator.PresentFlowCoordinator(notesFlowCoordinator);
}