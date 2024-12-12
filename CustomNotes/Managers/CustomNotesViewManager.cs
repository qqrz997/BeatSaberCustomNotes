using CustomNotes.Settings.UI;
using System;
using Zenject;

namespace CustomNotes.Managers
{
    internal class CustomNotesViewManager : IInitializable, IDisposable
    {
        private NoteListViewController noteListViewController;
        private NoteModifierViewController noteModifierViewController;
        private GameplaySetupViewController gameplaySetupViewController;

        public CustomNotesViewManager(NoteListViewController noteListViewController, NoteModifierViewController noteModifierViewController, GameplaySetupViewController gameplaySetupViewController)
        {
            this.noteListViewController = noteListViewController;
            this.noteModifierViewController = noteModifierViewController;
            this.gameplaySetupViewController = gameplaySetupViewController;
        }

        public void Initialize()
        {
            noteListViewController.CustomNotesReloaded += NoteListViewController_CustomNotesReloaded;
            gameplaySetupViewController.didActivateEvent += GameplaySetupViewController_DidActivateEvent;
        }

        public void Dispose()
        {
            noteListViewController.CustomNotesReloaded -= NoteListViewController_CustomNotesReloaded;
            gameplaySetupViewController.didActivateEvent -= GameplaySetupViewController_DidActivateEvent;
        }

        private void NoteListViewController_CustomNotesReloaded()
        {
            noteModifierViewController.SetupList();
        }

        private void GameplaySetupViewController_DidActivateEvent(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            noteModifierViewController.ParentControllerActivated();
        }
    }
}
