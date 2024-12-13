using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using CustomNotes.Data;
using CustomNotes.Utilities;
using HMUI;
using UnityEngine;
using Zenject;
using static IPA.Utilities.Utils;

namespace CustomNotes.Settings.UI
{
    internal class NoteDetailsViewController : BSMLResourceViewController
    {
        public override string ResourceName => "CustomNotes.Settings.UI.Views.noteDetails.bsml";

        [Inject] private readonly PluginConfig pluginConfig = null!;
        [Inject] private readonly NoteListViewController listViewController = null!;

        [UIComponent("note-description")] private readonly TextPageScrollView noteDescription = null;
        [UIObject("disable-april-fools")] private readonly GameObject disableAprilFoolsCheckbox = null!;

        public void OnNoteWasChanged(CustomNote customNote)
        {
            noteDescription.SetText(!string.IsNullOrWhiteSpace(customNote.ErrorMessage) ? string.Empty 
                : $"{customNote.Descriptor.NoteName}:\n\n{Utils.SafeUnescape(customNote.Descriptor.Description)}");

            NotifyPropertyChanged(nameof(ModEnabled));
            NotifyPropertyChanged(nameof(NoteSize));
            NotifyPropertyChanged(nameof(HmdOnly));
            NotifyPropertyChanged(nameof(AutoDisable));
        }

        [UIValue("mod-enabled")]
        public bool ModEnabled
        {
            get => pluginConfig.Enabled;
            set
            {
                pluginConfig.Enabled = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("note-size")]
        public float NoteSize
        {
            get => pluginConfig.NoteSize;
            set 
            { 
                pluginConfig.NoteSize = value;
                listViewController.ScalePreviewNotes(value);
                NotifyPropertyChanged();
            }
        }

        [UIValue("hmd-only")]
        public bool HmdOnly 
        {
            get => pluginConfig.HmdOnly;
            set 
            { 
                pluginConfig.HmdOnly = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("auto-disable")]
        public bool AutoDisable
        {
            get => pluginConfig.AutoDisable;
            set
            {
                pluginConfig.AutoDisable = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("disable-april-fools")]
        public bool DisableAprilFools
        {
            get => pluginConfig.DisableAprilFools;
            set
            {
                pluginConfig.DisableAprilFools = value;
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
}
