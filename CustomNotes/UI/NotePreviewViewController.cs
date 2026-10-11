using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using CustomNotes.Models;
using CustomNotes.Utilities;
using HMUI;

namespace CustomNotes.UI;

internal class NotePreviewViewController : BSMLResourceViewController
{
    public override string ResourceName => "CustomNotes.Resources.BSML.notePreview.bsml";

    [UIComponent("error-description")] private readonly TextPageScrollView errorDescription = null!;

    public void OnNoteWasChanged(CustomNoteData customNote)
    {
        if (string.IsNullOrWhiteSpace(customNote.Metadata.ErrorMessage))
        {
            errorDescription.gameObject.SetActive(false);
            return;
        }

        errorDescription.gameObject.SetActive(true);
        errorDescription.SetText($"{customNote.Metadata.Info.Name}:\n\n" +
                                 $"{Utils .SafeUnescape(customNote.Metadata.ErrorMessage)}");
    }
}