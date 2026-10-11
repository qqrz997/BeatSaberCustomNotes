using System;
using System.Collections;
using System.Collections.Generic;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.ViewControllers;
using CustomNotes.Managers;
using CustomNotes.Models;
using CustomNotes.Utilities;
using HMUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
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

    private bool isGeneratingPreview;
    private GameObject preview;

    // Notes
    private NoteInstance leftNoteArrow;
    private NoteInstance leftNoteDot;
    private NoteInstance rightNoteArrow;
    private NoteInstance rightNoteDot;
    private NoteInstance noteBomb;

    // NoteArrows
    private GameObject fakeLeftArrow;
    private GameObject fakeLeftDot;
    private GameObject fakeRightArrow;
    private GameObject fakeRightDot;

    // NotePositions (Local to the previewer)
    private readonly Vector3 leftArrowPos = new(0.0f, 0.0f, 0.0f);
    private readonly Vector3 leftDotPos = new(0.0f, 1.5f, 0.0f);
    private readonly Vector3 rightDotPos = new(1.5f, 1.5f, 0.0f);
    private readonly Vector3 rightArrowPos = new(1.5f, 0.0f, 0.0f);
    private readonly Vector3 bombPos = new(3.0f, 0.75f, 0.0f);
    
    public Action<CustomNoteData> CustomNoteChanged;
    public Action CustomNotesReloaded;
        
    [UIParams] private readonly BSMLParserParams parserParams = null!;
        
    [UIComponent("noteList")] private readonly CustomListTableData customListTableData = null!;
    [UIComponent("openedText")] private readonly TextMeshProUGUI openedText = null!;
    [UIComponent("donateButton")] private readonly Button donateButton = null!;

    [UIAction("reloadNotes")]
    public void ReloadNotes()
    {
        noteAssetLoader.Reload();
        SetupList();
        Select(customListTableData.TableView, GetSelectedIndex());
        CustomNotesReloaded?.Invoke();
    }

    [UIAction("donateClicked")]
    public void DonateClicked()
    {
        parserParams.EmitEvent("close-patreonModal");
        openedText.gameObject.SetActive(true);
        donateButton.interactable = false;
        Application.OpenURL("https://www.patreon.com/bobbievr");
        StartCoroutine(DonateActiveAgain());
    }

    [UIAction("closePressed")]
    public void ClosePressed()
    {
        parserParams.EmitEvent("close-patreonModal");
    }

    [UIAction("donateHelpClicked")]
    public void DonateHelpClicked()
    {
        parserParams.EmitEvent("open-patreonModal");
    }

    private IEnumerator DonateActiveAgain()
    {
        yield return new WaitForSeconds(3);
        donateButton.interactable = true;
        openedText.gameObject.SetActive(false);
    }
    
    [UIAction("noteSelect")]
    public void Select(TableView _, int row)
    {
        var cell = customListTableData.Data[row];
        var hash = cellValueMap.GetValueOrDefault(cell, null);
        config.LastNote = hash;
        
        var selectedNote = hash is null ? null : noteAssetLoader.LoadedCustomNotes.GetValueOrDefault(hash, null);
        GenerateNotePreview(selectedNote);
    }

    [UIAction("#post-parse")]
    public void SetupList()
    {
        customListTableData.Data.Clear();

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

        customListTableData.TableView.ReloadData();
        
        int selectedNote = GetSelectedIndex();
        customListTableData.TableView.ScrollToCellWithIdx(selectedNote, TableView.ScrollPositionType.Beginning, false) ;
        customListTableData.TableView.SelectCellWithIdx(selectedNote);
    }

    protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
    {
        base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);

        openedText.gameObject.SetActive(false);

        // if (fakeNoteArrows == null)
        // {
        //     byte[] resource = Utils.LoadFromResource("CustomNotes.Resources.Notes.cn_arrowplaceholder.bloq");
        //     fakeNoteArrows = NoteAssetLoader.LoadInternal(resource, "cn_arrowplaceholder.bloq");
        // }

        if (preview == null)
        {
            preview = new("NotePreviewContainer");
            preview.transform.localScale = new(0.3f, 0.3f, 0.3f);
            preview.transform.Rotate(0.0f, 57.0f, 0.0f);
        }

        int selectedNote = GetSelectedIndex();
        customListTableData.TableView.SelectCellWithIdx(selectedNote);
        Select(customListTableData.TableView, selectedNote);
    }

    protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
    {
        base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
        ClearPreview();
    }

    private int GetSelectedIndex() =>
        config.LastNote == null ? 0 : valueIndexMap.GetValueOrDefault(config.LastNote, 0);

    private void GenerateNotePreview(CustomNoteData note)
    {
        if (isGeneratingPreview)
        {
            return;
        }

        isGeneratingPreview = true;
            
        try
        {
            ClearNotes();
            
            if (note != null)
            {
                CustomNoteChanged?.Invoke(note);
                InitializePreviewNotes(note.Prefab, preview.transform);
            }
        }
        catch (Exception ex)
        {
            Plugin.Log.Error(ex);
        }

        isGeneratingPreview = false;
    }

    private void InitializePreviewNotes(CustomNotePrefab prefab, Transform transform)
    {
        leftNoteArrow = prefab.Left.NoteArrow.Instantiate();
        rightNoteArrow = prefab.Right.NoteArrow.Instantiate();
        leftNoteDot = prefab.Left.NoteDot.Instantiate();
        rightNoteDot = prefab.Right.NoteDot.Instantiate();
        noteBomb = prefab.Bomb.Instantiate();
        
        // Position previewer based on the CustomNote having a NoteBomb
        preview.transform.position = noteBomb != null ? new(2.60f, 0.90f, 2.50f) : new Vector3(2.75f, 0.9f, 2.25f);

        SetPreviewPosition(leftNoteArrow, transform, leftArrowPos);
        SetPreviewPosition(rightNoteArrow, transform, rightArrowPos);
        SetPreviewPosition(leftNoteDot, transform, leftDotPos);
        SetPreviewPosition(rightNoteDot, transform, rightDotPos);
        SetPreviewPosition(noteBomb, transform, bombPos);

        // Fake the Note Dots if no CustomNote Dot existed in the CustomNote
        // if (leftNoteArrow && !leftNoteDot)
        // {
        //     leftNoteDot = SetPreviewPosition(prefab.Left.NoteArrow.Instantiate().GameObject, transform, leftDotPos);
        // }
        // if (rightNoteArrow && !rightNoteDot)
        // {
        //     rightNoteDot = SetPreviewPosition(prefab.Right.NoteArrow.Instantiate().GameObject, transform, rightDotPos);
        // }

        // Add arrows to arrow-less notes
        // if (!customNote.HasArrows && fakeNoteArrows != null && noteLeft && noteRight)
        // {
        //     AddNoteArrows(fakeNoteArrows, transform);
        // }

        float colorStrength = 1f; // cn.Descriptor.NoteColorStrength;
        var noteAColor = gameplaySetupViewController.colorSchemesSettings.GetSelectedColorScheme().saberAColor;
        var noteBColor = gameplaySetupViewController.colorSchemesSettings.GetSelectedColorScheme().saberBColor;
    
        leftNoteArrow.SetColor(noteAColor * colorStrength);
        rightNoteArrow.SetColor(noteBColor * colorStrength);
        leftNoteDot.SetColor(noteAColor * colorStrength);
        rightNoteDot.SetColor(noteBColor * colorStrength);
    }

    private void SetPreviewPosition(NoteInstance note, Transform parent, Vector3 localPosition)
    {
        if (note == null) return;
        note.gameObject.transform.SetParent(parent, false);
        PositionPreviewNote(localPosition, note.gameObject);
    }

    // private void AddNoteArrows(ICustomNote customNote, Transform transform)
    // {
    //     fakeLeftArrow = CreatePreviewNote(customNote.Left.NoteArrow, transform, leftArrowPos);
    //     fakeLeftDot = CreatePreviewNote(customNote.Left.NoteDot, transform, leftDotPos);
    //     fakeRightArrow = CreatePreviewNote(customNote.Right.NoteArrow, transform, rightArrowPos);
    //     fakeRightDot = CreatePreviewNote(customNote.Right.NoteDot, transform, rightDotPos);
    // }

    private void PositionPreviewNote(Vector3 vector, GameObject noteObject)
    {
        if (noteObject == null) return;
        noteObject.transform.localPosition = vector;
        noteObject.transform.localScale *= config.GetNoteSize();
    }

    private void ClearPreview()
    {
        ClearNotes();
        DestroyGameObject(ref preview);
    }

    private void ClearNotes()
    {
        DestroyNote(ref leftNoteArrow);
        DestroyNote(ref rightNoteArrow);
        DestroyNote(ref leftNoteDot);
        DestroyNote(ref rightNoteDot);
        DestroyNote(ref noteBomb);
        DestroyGameObject(ref fakeLeftArrow);
        DestroyGameObject(ref fakeLeftDot);
        DestroyGameObject(ref fakeRightArrow);
        DestroyGameObject(ref fakeRightDot);
    }

    private static void DestroyNote(ref NoteInstance note)
    {
        if (!note) return;
        Destroy(note.gameObject);
        note = null;
    }
    
    private static void DestroyGameObject(ref GameObject o)
    {
        if (!o) return;
        Destroy(o);
        o = null;
    }
        
    private static void ScalePreviewNote(GameObject note, float scale)
    {
        if (note) note.transform.localScale = new Vector3(1,1,1) * scale;
    }
    public void ScalePreviewNotes(float scale)
    {
        ScalePreviewNote(leftNoteArrow.gameObject, scale);
        ScalePreviewNote(leftNoteDot.gameObject, scale);
        ScalePreviewNote(rightNoteArrow.gameObject, scale);
        ScalePreviewNote(rightNoteDot.gameObject, scale);
        ScalePreviewNote(noteBomb.gameObject, scale);

        ScalePreviewNote(fakeLeftArrow, scale);
        ScalePreviewNote(fakeLeftDot, scale);
        ScalePreviewNote(fakeRightArrow, scale);
        ScalePreviewNote(fakeRightDot, scale);
    }
}