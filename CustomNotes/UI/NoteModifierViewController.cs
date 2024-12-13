using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.GameplaySetup;
using CustomNotes.Managers;
using Zenject;

namespace CustomNotes.UI;

/*
 * View Controller for the Custom Notes selection found under the Mods tab in the Modifiers View
 * Allows for hotswapping notes without going to main menu or leaving TA lobby
 */
internal class NoteModifierViewController : IInitializable, IDisposable, INotifyPropertyChanged
{
    private readonly PluginConfig config;
    private readonly NoteAssetLoader noteAssetLoader;
    private readonly GameplaySetup gameplaySetup;
        
    [UIValue("notes-list")] private readonly List<object> notesList = [];
    [UIComponent("notes-dropdown")] private readonly DropDownListSetting notesDropdown = null!;

    public NoteModifierViewController(PluginConfig config, NoteAssetLoader noteAssetLoader, GameplaySetup gameplaySetup)
    {
        this.config = config;
        this.noteAssetLoader = noteAssetLoader;
        this.gameplaySetup = gameplaySetup;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public void Initialize()
    {
        SetupList();
        gameplaySetup.AddTab("Custom Notes", "CustomNotes.Resources.BSML.noteModifier.bsml", this);
    }

    public void Dispose()
    {
        gameplaySetup.RemoveTab("Custom Notes");
    }

    internal void ParentControllerActivated()
    {
        notesDropdown?.ReceiveValue();
        PropertyChanged?.Invoke(this, new(nameof(ModEnabled)));
        PropertyChanged?.Invoke(this, new(nameof(NoteSize)));
        PropertyChanged?.Invoke(this, new(nameof(HmdOnly)));
        PropertyChanged?.Invoke(this, new(nameof(AutoDisable)));
    }

    public void SetupList()
    {
        notesList.Clear();
        foreach (var customNote in noteAssetLoader.CustomNoteObjects)
        {
            notesList.Add(customNote.Descriptor.NoteName);
        }

        if (notesDropdown != null)
        {
            notesDropdown.Values = notesList;
            notesDropdown.UpdateChoices();
        }
    }

    [UIAction("note-selected")]
    public void OnSelect(string selectedCell)
    {
        int selectedNote = noteAssetLoader.CustomNoteObjects
            .ToList()
            .FindIndex(note => note.Descriptor.NoteName == selectedCell);
        noteAssetLoader.SelectedNoteIdx = selectedNote;
        config.LastNote = noteAssetLoader.CustomNoteObjects[selectedNote].FileName;
    }

    [UIValue("mod-enabled")]
    public bool ModEnabled
    {
        get => config.Enabled;
        set
        {
            config.Enabled = value;
            PropertyChanged?.Invoke(this, new(nameof(ModEnabled)));
        }
    }

    [UIValue("selected-note")]
    private string SelectedNote =>
        // Only select if valid bloq is loaded
        noteAssetLoader.CustomNoteObjects[noteAssetLoader.SelectedNoteIdx].ErrorMessage == null ? "Default" 
            : noteAssetLoader.CustomNoteObjects[noteAssetLoader.SelectedNoteIdx].Descriptor.NoteName;

    [UIValue("note-size")]
    public float NoteSize
    {
        get => config.NoteSize;
        set
        {
            config.NoteSize = value;
            PropertyChanged?.Invoke(this, new(nameof(NoteSize)));
        }
    }

    [UIValue("hmd-only")]
    public bool HmdOnly
    {
        get => config.HmdOnly;
        set
        {
            config.HmdOnly = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HmdOnly)));
        }
    }

    [UIValue("auto-disable")]
    public bool AutoDisable
    {
        get => config.AutoDisable;
        set
        {
            config.AutoDisable = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoDisable)));
        }
    }
}