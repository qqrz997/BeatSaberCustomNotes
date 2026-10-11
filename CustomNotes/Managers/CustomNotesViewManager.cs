using System;
using CustomNotes.UI;
using Zenject;

namespace CustomNotes.Managers;

internal class CustomNotesViewManager : IInitializable, IDisposable
{
    private readonly NoteListViewController noteListViewController;
    private readonly NoteModifierViewController noteModifierViewController;
    private readonly GameplaySetupViewController gameplaySetupViewController;

    private CustomNotesViewManager(NoteListViewController noteListViewController, NoteModifierViewController noteModifierViewController, GameplaySetupViewController gameplaySetupViewController)
    {
        this.noteListViewController = noteListViewController;
        this.noteModifierViewController = noteModifierViewController;
        this.gameplaySetupViewController = gameplaySetupViewController;
    }

    public void Initialize()
    {
        gameplaySetupViewController.didActivateEvent += GameplaySetupViewControllerDidActivateEvent;
        gameplaySetupViewController.didDeactivateEvent += GameplaySetupViewControllerDidDeactivateEvent;
    }

    public void Dispose()
    {
        gameplaySetupViewController.didActivateEvent -= GameplaySetupViewControllerDidActivateEvent;
        gameplaySetupViewController.didDeactivateEvent -= GameplaySetupViewControllerDidDeactivateEvent;
    }

    private void GameplaySetupViewControllerDidActivateEvent(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
    {
        noteModifierViewController.ParentControllerActivated();
    }

    private void GameplaySetupViewControllerDidDeactivateEvent(bool removedFromHierarchy, bool screenSystemDisabling)
    {
        noteModifierViewController.ParentControllerDeactivated();
    }
}