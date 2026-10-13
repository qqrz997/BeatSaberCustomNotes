using System;
using System.Collections.Generic;
using System.Linq;
using CustomNotes.Models;

namespace CustomNotes.Managers;

internal class NotesLoader
{
    private readonly LegacyCustomNoteLoader legacyCustomNoteLoader;

    public NotesLoader(LegacyCustomNoteLoader legacyCustomNoteLoader)
    {
        this.legacyCustomNoteLoader = legacyCustomNoteLoader;
    }
    
    public IEnumerable<CustomNoteData> LoadCustomNotes(IEnumerable<NoteFileInfo> customNoteFiles) => 
        customNoteFiles.Select(LoadCustomNote);

    private CustomNoteData LoadCustomNote(NoteFileInfo noteFile) => noteFile.FileInfo.Extension switch
    {
        // ".note2" => ,
        ".bloq" or ".note" => legacyCustomNoteLoader.LoadCustomNote(noteFile),
        _ => throw new NotSupportedException("Tried to load unsupported note file type")
    };
}