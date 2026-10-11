using System;
using System.Collections.Generic;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.ViewControllers;
using CustomNotes.Managers;
using CustomNotes.Models;
using CustomNotes.Utilities;
using HMUI;
using UnityEngine;
using Zenject;

namespace CustomNotes.UI;

[HotReload(RelativePathToLayout = "./BSML/noteList.bsml")]
[ViewDefinition("CustomNotes.UI.BSML.noteList.bsml")]
internal class NoteListViewController : BSMLAutomaticViewController
{
    [Inject] private readonly PluginConfig config = null!;
    [Inject] private readonly NoteAssetLoader noteAssetLoader = null!;
    [Inject] private readonly GameplaySetupViewController gameplaySetupViewController = null!;

    private readonly Dictionary<CustomListTableData.CustomCellInfo, string> cellValueMap = new();
    private readonly Dictionary<string, int> valueIndexMap = new();

    private bool isReloading;
    
    private readonly NotePreview preview = new();
    
    public Action<CustomNoteData> CustomNoteChanged;
        
    [UIParams] private readonly BSMLParserParams parserParams = null!;
        
    [UIComponent("noteList")] private readonly CustomListTableData customListTableData = null!;
    
    [UIAction("#post-parse")]
    public void PostParse()
    {
        SetupList();
    }

    protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
    {
        base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);

        noteAssetLoader.CustomNotesChanged += CustomNotesChanged;

        int selectedNote = GetSelectedIndex();
        customListTableData.TableView.SelectCellWithIdx(selectedNote);
        NoteListCellSelected(customListTableData.TableView, selectedNote);
    }

    protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
    {
        base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
        noteAssetLoader.CustomNotesChanged -= CustomNotesChanged;
        preview.ClearNotes();
    }
    
    public async void ReloadButtonClicked()
    {
        try
        {
            isReloading = true;
            await noteAssetLoader.Reload();
        }
        catch (Exception ex)
        {
            Plugin.Log.Warn($"Problem encountered during reload\n{ex}");
        }
        isReloading = false;
        
        SetupList();
    }

    public void NoteListCellSelected(TableView _, int row)
    {
        if (row < 0 || row >= customListTableData.Data.Count)
            return;
        
        preview.ClearNotes();
        
        var cell = customListTableData.Data[row];
        var hash = cellValueMap.GetValueOrDefault(cell, null);
        config.LastNote = hash;
        
        var selectedNote = hash is null ? null : noteAssetLoader.LoadedCustomNotes.GetValueOrDefault(hash, null);
        if (selectedNote != null)
        {
            CustomNoteChanged?.Invoke(selectedNote);
            InitializePreviewNotes(selectedNote.Prefab);
        }
    }
    
    public void SetupList()
    {
        customListTableData.Data.Clear();
        valueIndexMap.Clear();
        cellValueMap.Clear();

        if (!isReloading)
        {
            var defIcon = Utils.GetDefaultIcon();
            var defSprite = Sprite.Create(defIcon, new(0, 0, defIcon.width, defIcon.height), new(0.5f, 0.5f));
            customListTableData.Data.Add(new("Default", "Beat Games", defSprite));

            int i = 1;
            foreach (var (hash, customNote) in noteAssetLoader.LoadedCustomNotes)
            {
                var icon = customNote.Metadata.Info.Icon;
                var name = customNote.Metadata.Info.Name;
                var subName = customNote.Metadata.Info.Author;
                var sprite = Sprite.Create(icon, new(0, 0, icon.width, icon.height), new(0.5f, 0.5f));
                var cell = new CustomListTableData.CustomCellInfo(name, subName, sprite);
                customListTableData.Data.Add(cell);
                valueIndexMap.Add(hash, i);
                cellValueMap.Add(cell, hash);
                i++;
            }
        }

        customListTableData.TableView.ReloadData();
        
        int selectedNote = GetSelectedIndex();
        customListTableData.TableView.ScrollToCellWithIdx(selectedNote, TableView.ScrollPositionType.Beginning, false) ;
        customListTableData.TableView.SelectCellWithIdx(selectedNote);
    }
    
    public void ScalePreviewNotes(float scale)
    {
        preview.SetScale(scale);
    }

    private void CustomNotesChanged(IEnumerable<CustomNoteData> noteData)
    {
        SetupList();
    }

    private int GetSelectedIndex() =>
        config.LastNote == null ? 0 : valueIndexMap.GetValueOrDefault(config.LastNote, 0);

    private void InitializePreviewNotes(CustomNotePrefab prefab)
    {
        var leftNoteArrow = prefab.Left.NoteArrow.Instantiate();
        var rightNoteArrow = prefab.Right.NoteArrow.Instantiate();
        var leftNoteDot = prefab.Left.NoteDot.Instantiate();
        var rightNoteDot = prefab.Right.NoteDot.Instantiate();
        var noteBomb = prefab.Bomb.Instantiate();

        float colorStrength = 1f; // cn.Descriptor.NoteColorStrength;
        var noteAColor = gameplaySetupViewController.colorSchemesSettings.GetSelectedColorScheme().saberAColor;
        var noteBColor = gameplaySetupViewController.colorSchemesSettings.GetSelectedColorScheme().saberBColor;
    
        leftNoteArrow.SetColor(noteAColor * colorStrength);
        rightNoteArrow.SetColor(noteBColor * colorStrength);
        leftNoteDot.SetColor(noteAColor * colorStrength);
        rightNoteDot.SetColor(noteBColor * colorStrength);

        preview.LeftNoteArrow.SetNote(leftNoteArrow, config.NoteSize);
        preview.RightNoteArrow.SetNote(rightNoteArrow, config.NoteSize);
        preview.LeftNoteDot.SetNote(leftNoteDot, config.NoteSize);
        preview.RightNoteDot.SetNote(rightNoteDot, config.NoteSize);
        preview.NoteBomb.SetNote(noteBomb, config.NoteSize);
    }
}