using System;
using System.Collections.Generic;
using System.ComponentModel;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.GameplaySetup;
using CustomNotes.Managers;
using CustomNotes.Models;
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

    private NoteModifierViewController(PluginConfig config, NoteAssetLoader noteAssetLoader, GameplaySetup gameplaySetup)
    {
        this.config = config;
        this.noteAssetLoader = noteAssetLoader;
        this.gameplaySetup = gameplaySetup;
    }

    private bool parsed;
    
    [UIValue("notes-list")] private readonly List<object> notesList = [];
    [UIComponent("notes-dropdown")] private readonly DropDownListSetting notesDropdown = null!;

    public event PropertyChangedEventHandler? PropertyChanged;

    [UIAction("#post-parse")]
    public void PostParse()
    {
        parsed = true;
    }
    
    public void Initialize()
    {
        SetupList();
        gameplaySetup.AddTab("Custom Notes", "CustomNotes.UI.BSML.noteModifier.bsml", this);
    }

    public void Dispose()
    {
        gameplaySetup.RemoveTab("Custom Notes");
    }

    public void ParentControllerActivated()
    {
        noteAssetLoader.CustomNotesChanged += CustomNotesChanged;
        if (parsed) notesDropdown.ReceiveValue();
        PropertyChanged?.Invoke(this, new(nameof(ModEnabled)));
        PropertyChanged?.Invoke(this, new(nameof(NoteSize)));
        PropertyChanged?.Invoke(this, new(nameof(HmdOnly)));
        PropertyChanged?.Invoke(this, new(nameof(AutoDisable)));
    }

    public void ParentControllerDeactivated()
    {
        noteAssetLoader.CustomNotesChanged -= CustomNotesChanged;
    }

    public void SelectionChanged(object selectedCell)
    {
        config.LastNote = selectedCell is DropdownListItem item ? item.NoteData.Metadata.File.Hash : null;
    }

    public bool ModEnabled
    {
        get => config.Enabled;
        set
        {
            config.Enabled = value;
            PropertyChanged?.Invoke(this, new(nameof(ModEnabled)));
        }
    }

    public object DropdownValue => GetSelectedNote();
    private object GetSelectedNote()
    {
        foreach (var value in notesDropdown.Values)
            if (value is DropdownListItem item && item.NoteData.Metadata.File.Hash == config.LastNote)
                return value;
        return "Default";
    }

    public float NoteSize
    {
        get => config.NoteSize;
        set
        {
            config.NoteSize = value;
            PropertyChanged?.Invoke(this, new(nameof(NoteSize)));
        }
    }

    public bool HmdOnly
    {
        get => config.HmdOnly;
        set
        {
            config.HmdOnly = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HmdOnly)));
        }
    }

    public bool AutoDisable
    {
        get => config.AutoDisable;
        set
        {
            config.AutoDisable = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoDisable)));
        }
    }

    private void CustomNotesChanged(IEnumerable<CustomNoteData> noteData)
    {
        SetupList();
    }

    private void SetupList()
    {
        if (!parsed) return;
        
        notesList.Clear();
        notesList.Add("Default");
        foreach (var customNote in noteAssetLoader.LoadedCustomNotes)
        {
            notesList.Add(new DropdownListItem(customNote));
        }

        notesDropdown.Values = notesList;
        notesDropdown.UpdateChoices();
    }
    
    private class DropdownListItem
    {
        public DropdownListItem(CustomNoteData noteData) => NoteData = noteData;
        public CustomNoteData NoteData { get; }
        public override string ToString() => NoteData.Metadata.Info.Name;
    }
}