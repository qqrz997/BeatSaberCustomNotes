using BeatSaberMarkupLanguage;
using HMUI;
using Zenject;

namespace CustomNotes.UI;

internal class NotesFlowCoordinator : FlowCoordinator
{
    [Inject] private readonly MainFlowCoordinator mainFlow = null!;
    [Inject] private readonly NoteListViewController noteListView = null!;
    [Inject] private readonly NoteDetailsViewController noteDetailsView = null!;
    [Inject] private readonly NotePreviewViewController notePreviewView = null!;

    protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
    {
        if (firstActivation)
        {
            SetTitle("Custom Notes");
            showBackButton = true;
        }
        ProvideInitialViewControllers(noteListView, noteDetailsView, notePreviewView);
        noteListView.CustomNoteChanged += noteDetailsView.OnNoteWasChanged;
        noteListView.CustomNoteChanged += notePreviewView.OnNoteWasChanged;
    }

    protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
    {
        noteListView.CustomNoteChanged -= noteDetailsView.OnNoteWasChanged;
        noteListView.CustomNoteChanged -= notePreviewView.OnNoteWasChanged;
        base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
    }

    protected override void BackButtonWasPressed(ViewController topViewController)
    {
        // Dismiss ourselves
        mainFlow.DismissFlowCoordinator(this, null);
    }
}