using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using CustomNotes.Models;

namespace CustomNotes.Managers;

internal interface INoteDataLoader
{
    IEnumerable<CustomNoteData> LoadedCustomNotes { get; }
    
    event Action<IEnumerable<CustomNoteData>>? CustomNotesChanged;
    
    bool TryGetCustomNote(string? hash, [NotNullWhen(true)] out CustomNoteData? note);
    
    Task Load(CancellationToken token);
    Task Reload();
    void Clear();
}