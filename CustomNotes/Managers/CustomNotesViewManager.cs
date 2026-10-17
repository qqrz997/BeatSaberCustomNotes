using System;
using CustomNotes.UI;
using Zenject;

namespace CustomNotes.Managers;

internal class CustomNotesViewManager : IInitializable, IDisposable
{
    private readonly NoteModifierViewController noteModifierViewController;
    private readonly GameplaySetupViewController gameplaySetupViewController;

    private CustomNotesViewManager(
        NoteModifierViewController noteModifierViewController,
        GameplaySetupViewController gameplaySetupViewController)
    {
        this.noteModifierViewController = noteModifierViewController;
        this.gameplaySetupViewController = gameplaySetupViewController;
    }

    public void Initialize()
    {
        gameplaySetupViewController.didActivateEvent += GameplaySetupViewControllerDidActivateEvent;
    }

    public void Dispose()
    {
        gameplaySetupViewController.didActivateEvent -= GameplaySetupViewControllerDidActivateEvent;
    }

    private void GameplaySetupViewControllerDidActivateEvent(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
    {
        noteModifierViewController.ParentControllerActivated();
    }
}