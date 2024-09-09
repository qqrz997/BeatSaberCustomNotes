using System;
using Zenject;
using BeatSaberMarkupLanguage;
using CustomNotes.Settings.UI;
using BeatSaberMarkupLanguage.MenuButtons;

namespace CustomNotes.Managers
{
    internal class MenuButtonManager : IInitializable, IDisposable
    {
        private readonly MenuButton _menuButton;
        private readonly MainFlowCoordinator _mainFlowCoordinator;
        private readonly NotesFlowCoordinator _notesFlowCoordinator;

        public MenuButtonManager(MainFlowCoordinator mainFlowCoordinator, NotesFlowCoordinator notesFlowCoordinator)
        {
            _mainFlowCoordinator = mainFlowCoordinator;
            _notesFlowCoordinator = notesFlowCoordinator;
            _menuButton = new MenuButton("Custom Notes", "Change Custom Notes Here!", ShowNotesFlow, true);
        }

        public void Initialize()
        {
            MenuButtons.Instance.RegisterButton(_menuButton);
        }

        public void Dispose()
        {
            MenuButtons.Instance.UnregisterButton(_menuButton);
        }

        private void ShowNotesFlow()
        {
            _mainFlowCoordinator.PresentFlowCoordinator(_notesFlowCoordinator);
        }
    }
}