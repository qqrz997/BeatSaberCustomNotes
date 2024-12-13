﻿using HMUI;
using System;
using System.Collections;
using Zenject;
using UnityEngine;
using UnityEngine.UI;
using CustomNotes.Data;
using CustomNotes.Managers;
using CustomNotes.Utilities;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.ViewControllers;
using TMPro;

namespace CustomNotes.Settings.UI
{
    internal class NoteListViewController : BSMLResourceViewController
    {
        public override string ResourceName => "CustomNotes.Settings.UI.Views.noteList.bsml";

        private PluginConfig pluginConfig;
        private NoteAssetLoader noteAssetLoader;
        private GameplaySetupViewController gameplaySetupViewController;

        private bool isGeneratingPreview;
        private GameObject preview;

        // Notes
        private GameObject noteLeft;
        private GameObject noteDotLeft;
        private GameObject noteRight;
        private GameObject noteDotRight;
        private GameObject noteBomb;

        // NoteArrows
        private CustomNote fakeNoteArrows;
        private GameObject fakeLeftArrow;
        private GameObject fakeLeftDot;
        private GameObject fakeRightArrow;
        private GameObject fakeRightDot;

        // NotePositions (Local to the previewer)
        private readonly Vector3 leftDotPos = new(0.0f, 1.5f, 0.0f);
        private readonly Vector3 leftArrowPos = new(0.0f, 0.0f, 0.0f);
        private readonly Vector3 rightDotPos = new(1.5f, 1.5f, 0.0f);
        private readonly Vector3 rightArrowPos = new(1.5f, 0.0f, 0.0f);
        private readonly Vector3 bombPos = new(3.0f, 0.75f, 0.0f);

        public Action<CustomNote> CustomNoteChanged;
        public Action CustomNotesReloaded;

        [Inject]
        public void Construct(PluginConfig pluginConfig, NoteAssetLoader noteAssetLoader, GameplaySetupViewController gameplaySetupViewController)
        {
            this.pluginConfig = pluginConfig;
            this.noteAssetLoader = noteAssetLoader;
            this.gameplaySetupViewController = gameplaySetupViewController;
        }
        
        [UIParams] private readonly BSMLParserParams parserParams = null!;
        
        [UIComponent("noteList")] private readonly CustomListTableData customListTableData = null!;
        [UIComponent("openedText")] private readonly TextMeshProUGUI openedText = null!;
        [UIComponent("donateButton")] private readonly Button donateButton = null!;

        [UIAction("noteSelect")]
        public void Select(TableView _, int row)
        {
            noteAssetLoader.SelectedNoteIdx = row;
            pluginConfig.LastNote = noteAssetLoader.CustomNoteObjects[row].FileName;

            GenerateNotePreview(row);
        }

        [UIAction("reloadNotes")]
        public void ReloadNotes()
        {
            noteAssetLoader.Reload();
            SetupList();
            Select(customListTableData.TableView, noteAssetLoader.SelectedNoteIdx);
            CustomNotesReloaded?.Invoke();
        }

        [UIAction("donateClicked")]
        public void DonateClicked()
        {
            //button.interactable = false;
            //linkOpened.gameObject.SetActive(true);
            //StartCoroutine(SecondRemove(button));
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
            //button.interactable = false;
            //linkOpened.gameObject.SetActive(true);
            //StartCoroutine(SecondRemove(button));
            parserParams.EmitEvent("open-patreonModal");
            //Application.OpenURL("https://www.patreon.com/bobbievr");
        }

        private IEnumerator DonateActiveAgain()
        {
            yield return new WaitForSeconds(3);
            donateButton.interactable = true;
            openedText.gameObject.SetActive(false);
        }

        [UIAction("#post-parse")]
        public void SetupList()
        {
            customListTableData.Data.Clear();

            foreach (var customNote in noteAssetLoader.CustomNoteObjects)
            {
                var icon = customNote.Descriptor.Icon;
                var sprite = Sprite.Create(icon, new(0, 0, icon.width, icon.height), new(0.5f, 0.5f));
                var customCellInfo = new CustomListTableData.CustomCellInfo(
                    customNote.Descriptor.NoteName, customNote.Descriptor.AuthorName, sprite);
                customListTableData.Data.Add(customCellInfo);
            }

            customListTableData.TableView.ReloadData();
            int selectedNote = noteAssetLoader.SelectedNoteIdx;

            customListTableData.TableView.ScrollToCellWithIdx(selectedNote, TableView.ScrollPositionType.Beginning, false) ;
            customListTableData.TableView.SelectCellWithIdx(selectedNote);
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);

            openedText.gameObject.SetActive(false);

            if (fakeNoteArrows == null)
            {
                byte[] resource = Utils.LoadFromResource("CustomNotes.Resources.Notes.cn_arrowplaceholder.bloq");
                fakeNoteArrows = CustomNote.LoadInternal(resource, "cn_arrowplaceholder.bloq");
            }

            if (preview == null)
            {
                preview = new("NotePreviewContainer");
                preview.transform.Rotate(0.0f, 60.0f, 0.0f);
                preview.transform.localScale = new(0.3f, 0.3f, 0.3f);
            }

            int selectedNote = noteAssetLoader.SelectedNoteIdx;
            customListTableData.TableView.SelectCellWithIdx(selectedNote);
            Select(customListTableData.TableView, selectedNote);
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
            ClearPreview();
        }

        private void GenerateNotePreview(int selectedNote)
        {
            if (isGeneratingPreview)
            {
                return;
            }

            isGeneratingPreview = true;
            
            try
            {
                ClearNotes();

                var currentNote = noteAssetLoader.CustomNoteObjects[selectedNote];
                if (currentNote != null)
                {
                    CustomNoteChanged?.Invoke(currentNote);
                    InitializePreviewNotes(currentNote, preview.transform);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.Error(ex);
            }

            isGeneratingPreview = false;
        }

        private void InitializePreviewNotes(CustomNote customNote, Transform transform)
        {
            // Position previewer based on the CustomNote having a NoteBomb
            preview.transform.position = customNote.NoteBomb ? new(3.05f, 0.9f, 2.0f) : new Vector3(2.90f, 0.9f, 1.85f);

            noteLeft = CreatePreviewNote(customNote.NoteLeft, transform, leftArrowPos);
            noteDotLeft = CreatePreviewNote(customNote.NoteDotLeft, transform, leftDotPos);
            noteRight = CreatePreviewNote(customNote.NoteRight, transform, rightArrowPos);
            noteDotRight = CreatePreviewNote(customNote.NoteDotRight, transform, rightDotPos);
            noteBomb = CreatePreviewNote(customNote.NoteBomb, transform, bombPos);

            // Fake the Note Dots if no CustomNote Dot existed in the CustomNote
            if (noteLeft && !noteDotLeft)
            {
                noteDotLeft = CreatePreviewNote(customNote.NoteLeft, transform, leftDotPos);
            }
            if (noteRight && !noteDotRight)
            {
                noteDotRight = CreatePreviewNote(customNote.NoteRight, transform, rightDotPos);
            }

            // Add arrows to arrow-less notes
            if (!customNote.Descriptor.DisableBaseNoteArrows && fakeNoteArrows != null)
            {
                if (noteLeft && noteRight)
                {
                    AddNoteArrows(fakeNoteArrows, transform);
                }
            }

            if (customNote.Descriptor.UsesNoteColor)
            {
                float colorStrength = customNote.Descriptor.NoteColorStrength;
                var noteAColor = gameplaySetupViewController.colorSchemesSettings.GetSelectedColorScheme().saberAColor;
                var noteBColor = gameplaySetupViewController.colorSchemesSettings.GetSelectedColorScheme().saberBColor;

                Utils.ColorizeCustomNote(noteAColor, colorStrength, noteLeft);
                Utils.ColorizeCustomNote(noteBColor, colorStrength, noteRight);
                Utils.ColorizeCustomNote(noteBColor, colorStrength, noteDotRight);
                Utils.ColorizeCustomNote(noteAColor, colorStrength, noteDotLeft);
            }
        }

        private GameObject CreatePreviewNote(GameObject baseNote, Transform transform, Vector3 localPosition)
        {
            if (baseNote == null)
            {
                return null;
            }
            
            var previewNoteObject = Instantiate(baseNote, transform);
            PositionPreviewNote(localPosition, previewNoteObject);
            
            return previewNoteObject;
        }

        private void AddNoteArrows(CustomNote customNote, Transform transform)
        {
            fakeLeftArrow = CreatePreviewNote(customNote.NoteLeft, transform, leftArrowPos);
            fakeLeftDot = CreatePreviewNote(customNote.NoteDotLeft, transform, leftDotPos);
            fakeRightArrow = CreatePreviewNote(customNote.NoteRight, transform, rightArrowPos);
            fakeRightDot = CreatePreviewNote(customNote.NoteDotRight, transform, rightDotPos);
        }

        private void PositionPreviewNote(Vector3 vector, GameObject noteObject)
        {
            if (noteObject != null)
            {
                noteObject.transform.localPosition = vector;
                noteObject.transform.localScale *= pluginConfig.GetNoteSize();
            }
        }

        private void ClearPreview()
        {
            ClearNotes();
            DestroyGameObject(ref preview);
        }

        private void ClearNotes()
        {
            DestroyGameObject(ref noteLeft);
            DestroyGameObject(ref noteDotLeft);
            DestroyGameObject(ref noteRight);
            DestroyGameObject(ref noteDotRight);
            DestroyGameObject(ref noteBomb);

            DestroyGameObject(ref fakeLeftArrow);
            DestroyGameObject(ref fakeLeftDot);
            DestroyGameObject(ref fakeRightArrow);
            DestroyGameObject(ref fakeRightDot);
        }

        private static void DestroyGameObject(ref GameObject gameObject)
        {
            if (gameObject)
            {
                Destroy(gameObject);
                gameObject = null;
            }
        }
        
        private static void ScalePreviewNote(GameObject note, float scale)
        {
            if (note)
            {
                note.transform.localScale = new Vector3(1,1,1) * scale;
            }
        }
        public void ScalePreviewNotes(float scale)
        {
            ScalePreviewNote(noteLeft, scale);
            ScalePreviewNote(noteDotLeft, scale);
            ScalePreviewNote(noteRight, scale);
            ScalePreviewNote(noteDotRight, scale);
            ScalePreviewNote(noteBomb, scale);

            ScalePreviewNote(fakeLeftArrow, scale);
            ScalePreviewNote(fakeLeftDot, scale);
            ScalePreviewNote(fakeRightArrow, scale);
            ScalePreviewNote(fakeRightDot, scale);
        }
    }
}