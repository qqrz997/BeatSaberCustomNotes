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
    [Inject] private readonly NotePreview preview = null!;
    [Inject] private readonly GameResourcesManager gameResourcesManager = null!;

    private readonly Dictionary<CustomListTableData.CustomCellInfo, string> cellValueMap = new();
    private readonly Dictionary<string, int> valueIndexMap = new();

    private bool isReloading;
    
    public Action<CustomNoteData> CustomNoteChanged;
        
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
        
        if (noteAssetLoader.TryGetCustomNote(hash, out var selectedNote))
        {
            CustomNoteChanged?.Invoke(selectedNote);
        }
        
        InitializePreviewNotes(selectedNote);
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
            foreach (var customNote in noteAssetLoader.LoadedCustomNotes)
            {
                var hash = customNote.Metadata.File.Hash;
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

    private void InitializePreviewNotes(CustomNoteData note)
    {
        preview.LeftNoteArrow.SetNote(note?.Prefab.Left.NoteArrow.Instantiate());
        preview.RightNoteArrow.SetNote(note?.Prefab.Right.NoteArrow.Instantiate());
        preview.LeftNoteDot.SetNote(note?.Prefab.Left.NoteDot.Instantiate());
        preview.RightNoteDot.SetNote(note?.Prefab.Right.NoteDot.Instantiate());
        preview.NoteBomb.SetNote(note?.Prefab.Bomb.Instantiate());

        var leftColor = gameplaySetupViewController.colorSchemesSettings.GetSelectedColorScheme().saberAColor;
        var rightColor = gameplaySetupViewController.colorSchemesSettings.GetSelectedColorScheme().saberBColor;
        var colorMultiplier = note?.Prefab.ColorMultiplier ?? 1f;
        
        preview.LeftNoteArrow.SetColor(leftColor, colorMultiplier);
        preview.RightNoteArrow.SetColor(rightColor, colorMultiplier);
        preview.LeftNoteDot.SetColor(leftColor, colorMultiplier);
        preview.RightNoteDot.SetColor(rightColor, colorMultiplier);

        ScalePreviewNotes(config.NoteSize);
    }
}