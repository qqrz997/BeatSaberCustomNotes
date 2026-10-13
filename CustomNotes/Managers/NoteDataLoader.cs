using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomNotes.Models;
using IPA.Utilities;
using SiraUtil.Zenject;
using UnityEngine;
using Object = UnityEngine.Object;
using Utils = CustomNotes.Utilities.Utils;

namespace CustomNotes.Managers;

internal class NoteDataLoader : IAsyncInitializable, IDisposable, INoteDataLoader
{
    private readonly NoteFileManager noteFileManager;
    private readonly NotesLoader notesLoader;

    private readonly string notesDirectory = Path.Combine(UnityGame.InstallPath, "CustomNotes");
    private readonly Dictionary<string, CustomNoteData> loadedCustomNotes = [];

    private NoteDataLoader(NoteFileManager noteFileManager, NotesLoader notesLoader)
    {
        this.noteFileManager = noteFileManager;
        this.notesLoader = notesLoader;
    }

    private bool isLoaded;
    private NoteFileInfo[] customNoteFiles = [];

    public IEnumerable<CustomNoteData> LoadedCustomNotes => loadedCustomNotes.Values;

    public event Action<IEnumerable<CustomNoteData>>? CustomNotesChanged; 
    
    public async Task InitializeAsync(CancellationToken token) => await Load(token);
    public void Dispose() => Clear();
    
    public bool TryGetCustomNote(string? hash, [NotNullWhen(true)] out CustomNoteData? note)
    {
        note = null;
        return !string.IsNullOrEmpty(hash) && loadedCustomNotes.TryGetValue(hash!, out note);
    }

    public async Task Load(CancellationToken token)
    {
        if (isLoaded)
        {
            return;
        }

        Directory.CreateDirectory(notesDirectory);

        customNoteFiles = await noteFileManager.ReloadAllFiles(token);
        Plugin.Log.Notice($"{customNoteFiles.Length} external notes found. Preparing to load.");
            
        foreach (var note in notesLoader.LoadCustomNotes(customNoteFiles))
        {
            loadedCustomNotes.Add(note.Metadata.File.Hash, note);
        }
        Plugin.Log.Notice($"{loadedCustomNotes.Count} total custom notes loaded.");
        
        CustomNotesChanged?.Invoke(LoadedCustomNotes);

        isLoaded = true;
    }
    
    public async Task Reload()
    {
        Clear();
        await InitializeAsync(CancellationToken.None);
    }
    
    public void Clear()
    {
        foreach (var customNote in LoadedCustomNotes)
            customNote.Dispose();
        loadedCustomNotes.Clear();
        CustomNotesChanged?.Invoke(LoadedCustomNotes);
        isLoaded = false;
    }
}