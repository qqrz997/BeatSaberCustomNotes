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

    public void OnNoteWasChanged(CustomNote customNote)
    {
        if (string.IsNullOrWhiteSpace(customNote.ErrorMessage))
        {
            errorDescription.gameObject.SetActive(false);
            return;
        }

        errorDescription.gameObject.SetActive(true);
        errorDescription.SetText($"{customNote.Descriptor?.NoteName}:\n\n{Utils.SafeUnescape(customNote
            .ErrorMessage)}");
    }
}