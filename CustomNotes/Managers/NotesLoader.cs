using System;
using System.Collections.Generic;
using System.Linq;
using CustomNotes.Models;

namespace CustomNotes.Managers;

internal class NotesLoader
{
    private readonly LegacyCustomNoteLoader legacyCustomNoteLoader;
    private readonly Note2Loader note2Loader;
    
    public NotesLoader(LegacyCustomNoteLoader legacyCustomNoteLoader, Note2Loader note2Loader)
    {
        this.legacyCustomNoteLoader = legacyCustomNoteLoader;
        this.note2Loader = note2Loader;
    }
    
    public IEnumerable<CustomNoteData> LoadCustomNotes(IEnumerable<NoteFileInfo> customNoteFiles) => 
        customNoteFiles.Select(LoadCustomNote);

    private CustomNoteData LoadCustomNote(NoteFileInfo noteFile) => noteFile.FileInfo.Extension switch
    {
        ".note2" => note2Loader.LoadNote2(noteFile),
        ".bloq" or ".note" => legacyCustomNoteLoader.LoadCustomNote(noteFile),
        _ => throw new NotSupportedException("Tried to load unsupported note file type")
    };
}