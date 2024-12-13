using HMUI;
using Zenject;
using BeatSaberMarkupLanguage;

namespace CustomNotes.Settings.UI;

internal class NotesFlowCoordinator : FlowCoordinator
{
    private MainFlowCoordinator mainFlow;
    private NoteListViewController noteListView;
    private NoteDetailsViewController noteDetailsView;
    private NotePreviewViewController notePreviewView;

    [Inject]
    public void Construct(MainFlowCoordinator mainFlow, NoteListViewController noteListView, NoteDetailsViewController noteDetailsView, NotePreviewViewController notePreviewView)
    {
        this.mainFlow = mainFlow;
        this.noteListView = noteListView;
        this.noteDetailsView = noteDetailsView;
        this.notePreviewView = notePreviewView;
    }

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