using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using CustomNotes.Models;
using HMUI;
using UnityEngine;
using Zenject;
using static IPA.Utilities.Utils;

namespace CustomNotes.UI;

[HotReload(RelativePathToLayout = "./BSML/noteDetails.bsml")]
[ViewDefinition("CustomNotes.UI.BSML.noteDetails.bsml")]
internal class NoteDetailsViewController : BSMLAutomaticViewController
{
    [Inject] private readonly PluginConfig config = null!;
    [Inject] private readonly NoteListViewController listViewController = null!;

    [UIObject("DisableAprilFools")] private readonly GameObject disableAprilFoolsCheckbox = null!;

    public void OnNoteWasChanged(CustomNoteData customNote)
    {
        NotifyPropertyChanged(nameof(ModEnabled));
        NotifyPropertyChanged(nameof(NoteSize));
        NotifyPropertyChanged(nameof(HmdOnly));
        NotifyPropertyChanged(nameof(AutoDisable));
    }

    public bool ModEnabled
    {
        get => config.Enabled;
        set
        {
            config.Enabled = value;
            NotifyPropertyChanged();
        }
    }

    public float NoteSize
    {
        get => config.NoteSize;
        set 
        { 
            config.NoteSize = value;
            listViewController.ScalePreviewNotes(value);
            NotifyPropertyChanged();
        }
    }

    public bool HmdOnly 
    {
        get => config.HmdOnly;
        set 
        { 
            config.HmdOnly = value;
            NotifyPropertyChanged();
        }
    }

    public bool AutoDisable
    {
        get => config.AutoDisable;
        set
        {
            config.AutoDisable = value;
            NotifyPropertyChanged();
        }
    }

    public bool DisableAprilFools
    {
        get => config.DisableAprilFools;
        set
        {
            config.DisableAprilFools = value;
            NotifyPropertyChanged();
        }
    }

    protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
    {
        base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);

        var time = CanUseDateTimeNowSafely ? System.DateTime.Now : System.DateTime.UtcNow;
        disableAprilFoolsCheckbox.gameObject.SetActive(time is { Month: 4, Day: 1 });
    }
}