﻿using CustomNotes.Managers;
using CustomNotes.Models;
using CustomNotes.Utilities;
using SiraUtil.Interfaces;
using SiraUtil.Objects;
using UnityEngine;
using Zenject;

namespace CustomNotes.Components;

public class CustomNoteController : MonoBehaviour, IColorable, INoteControllerNoteWasCutEvent, INoteControllerNoteWasMissedEvent, INoteControllerDidInitEvent, INoteControllerNoteDidDissolveEvent
{
    private PluginConfig config;

    protected Transform NoteCube;
    private CustomNote customNote;
    private GameNoteController gameNoteController;
    private CustomNoteColorNoteVisuals customNoteColorNoteVisuals;

    protected GameObject ActiveNote;
    protected SiraPrefabContainer Container;
    protected SiraPrefabContainer.Pool ActivePool;

    private SiraPrefabContainer.Pool leftDotNotePool;
    private SiraPrefabContainer.Pool rightDotNotePool;
    private SiraPrefabContainer.Pool leftArrowNotePool;
    private SiraPrefabContainer.Pool rightArrowNotePool;
    private SiraPrefabContainer.Pool leftBurstSliderHeadPool;
    private SiraPrefabContainer.Pool rightBurstSliderHeadPool;
    private SiraPrefabContainer.Pool leftBurstSliderHeadDotPool;
    private SiraPrefabContainer.Pool rightBurstSliderHeadDotPool;

    public Color Color
    {
        get => customNoteColorNoteVisuals != null ? customNoteColorNoteVisuals._noteColor : Color.white;
        set => SetColor(value);
    }

    [Inject]
    internal void Init(PluginConfig config,
        NoteAssetLoader noteAssetLoader,
        [Inject(Id = Protocol.LeftArrowPool)] SiraPrefabContainer.Pool leftArrowNotePool,
        [Inject(Id = Protocol.RightArrowPool)] SiraPrefabContainer.Pool rightArrowNotePool,
        [InjectOptional(Id = Protocol.LeftDotPool)] SiraPrefabContainer.Pool leftDotNotePool,
        [InjectOptional(Id = Protocol.RightDotPool)] SiraPrefabContainer.Pool rightDotNotePool,
        [Inject(Id = Protocol.LeftBurstSliderHeadPool)] SiraPrefabContainer.Pool leftBurstSliderHeadPool,
        [Inject(Id = Protocol.RightBurstSliderHeadPool)] SiraPrefabContainer.Pool rightBurstSliderHeadPool,
        [Inject(Id = Protocol.LeftBurstSliderHeadDotPool)] SiraPrefabContainer.Pool leftBurstSliderHeadDotPool,
        [Inject(Id = Protocol.RightBurstSliderHeadDotPool)] SiraPrefabContainer.Pool rightBurstSliderHeadDotPool)
    {
        this.config = config;
        this.leftArrowNotePool = leftArrowNotePool;
        this.rightArrowNotePool = rightArrowNotePool;

        this.leftDotNotePool = leftDotNotePool ?? this.leftArrowNotePool;
        this.rightDotNotePool = rightDotNotePool ?? this.rightArrowNotePool;

        this.leftBurstSliderHeadPool = leftBurstSliderHeadPool;
        this.rightBurstSliderHeadPool = rightBurstSliderHeadPool;
        this.leftBurstSliderHeadDotPool = leftBurstSliderHeadDotPool;
        this.rightBurstSliderHeadDotPool = rightBurstSliderHeadDotPool;

        customNote = noteAssetLoader.CustomNoteObjects[noteAssetLoader.SelectedNoteIdx];

        gameNoteController = GetComponent<GameNoteController>();
        customNoteColorNoteVisuals = gameObject.GetComponent<CustomNoteColorNoteVisuals>();
        customNoteColorNoteVisuals.enabled = true;

        gameNoteController.didInitEvent.Add(this);
        gameNoteController.noteWasCutEvent.Add(this);
        gameNoteController.noteWasMissedEvent.Add(this);
        gameNoteController.noteDidDissolveEvent.Add(this);
        customNoteColorNoteVisuals.didInitEvent += Visuals_DidInit;

        NoteCube = gameNoteController.gameObject.transform.Find("NoteCube");

        var noteMesh = GetComponentInChildren<MeshRenderer>();
        if (config.UseHmdOnly())
        {
            noteMesh.gameObject.layer = (int)NoteLayer.ThirdPerson;
        }
        else
        {
            // only disable if custom notes display on both hmd and display
            noteMesh.forceRenderingOff = true;
        }
    }

    public void HandleNoteControllerNoteWasMissed(NoteController noteController)
    {
        if (Container != null)
        {
            Container.transform.SetParent(null);
            
            if (noteController.noteData.colorType != ColorType.None)
            {
                Container.Prefab.SetActive(false);
                ActivePool?.Despawn(Container);
                Container = null;
            }
        }
    }

    public void HandleNoteControllerDidInit(NoteControllerBase noteController)
    {
        var data = noteController.noteData;
        if (data.gameplayType != NoteData.GameplayType.BurstSliderHead)
        {
            SpawnThenParent(data.colorType == ColorType.ColorA
                ? data.cutDirection == NoteCutDirection.Any ? leftDotNotePool : leftArrowNotePool
                : data.cutDirection == NoteCutDirection.Any ? rightDotNotePool : rightArrowNotePool);
        }
        else
        {
            SpawnThenParent(data.colorType == ColorType.ColorA
                ? data.cutDirection == NoteCutDirection.Any ? leftBurstSliderHeadDotPool : leftBurstSliderHeadPool
                : data.cutDirection == NoteCutDirection.Any ? rightBurstSliderHeadDotPool : rightBurstSliderHeadPool);
        }
    }

    private void SpawnThenParent(SiraPrefabContainer.Pool noteModelPool)
    {
        ActivePool = noteModelPool;
        Container = noteModelPool.Spawn();
        
        ActiveNote = Container.Prefab;
        ActiveNote.SetLayerRecursively(config.UseHmdOnly() ? NoteLayer.FirstPerson : NoteLayer.Note);
        ActiveNote.transform.localPosition = Vector3.zero;
        ActiveNote.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f) * config.GetNoteSize();
        ActiveNote.SetActive(true);
        
        Container.transform.SetParent(NoteCube);
        Container.transform.localPosition = Vector3.zero;
        Container.transform.localRotation = Quaternion.identity;
        Container.transform.localScale = Vector3.one;
    }

    protected void SetActiveThenColor(GameObject note, Color color)
    {
        note.SetActive(true);
        if (customNote.Descriptor.UsesNoteColor)
        {
            SetColor(color, true);
        }
    }

    private void Visuals_DidInit(ColorNoteVisuals visuals, NoteControllerBase noteController)
    {
        SetActiveThenColor(ActiveNote, ((CustomNoteColorNoteVisuals)visuals)._noteColor);
        // Hide certain parts of the default note which is not required
        if (!config.UseHmdOnly())
        {
            customNoteColorNoteVisuals.SetBaseGameVisualsLayer(NoteLayer.Note);
            if (customNote.Descriptor.DisableBaseNoteArrows)
            {
                customNoteColorNoteVisuals.TurnOffVisuals();
            }
            else if (!config.NoteSizeEquals(1))
            {
                customNoteColorNoteVisuals.ScaleVisuals(config.GetNoteSize());
            }

            return;
        }

        // HMDOnly code
        customNoteColorNoteVisuals.SetBaseGameVisualsLayer(NoteLayer.ThirdPerson);
        if (!customNote.Descriptor.DisableBaseNoteArrows)
        {
            if (!config.NoteSizeEquals(1))
            {
                // arrows should be enabled in both views, with fake arrows rescaled
                customNoteColorNoteVisuals.CreateAndScaleFakeVisuals(NoteLayer.FirstPerson, config.GetNoteSize());
            }
            else
            {
                // arrows should be enabled in both views
                customNoteColorNoteVisuals.CreateFakeVisuals(NoteLayer.FirstPerson);
            }
        }
    }

    protected void OnDestroy()
    {
        if (gameNoteController != null)
        {
            gameNoteController.didInitEvent.Remove(this);
            gameNoteController.noteWasCutEvent.Remove(this);
            gameNoteController.noteWasMissedEvent.Remove(this);
            gameNoteController.noteDidDissolveEvent.Remove(this);
        }
        if (customNoteColorNoteVisuals != null)
        {
            customNoteColorNoteVisuals.didInitEvent -= Visuals_DidInit;
        }
    }

    public void SetColor(Color color)
    {
        SetColor(color, false);
    }

    public void SetColor(Color color, bool updateMatBlocks)
    {
        if (ActiveNote != null)
        {
            customNoteColorNoteVisuals.SetColor(color, updateMatBlocks);
            Utils.ColorizeCustomNote(color, customNote.Descriptor.NoteColorStrength, ActiveNote);
        }
    }

    public void HandleNoteControllerNoteWasCut(NoteController nc, in NoteCutInfo _)
    {
        HandleNoteControllerNoteWasMissed(nc);
    }

    public void HandleNoteControllerNoteDidDissolve(NoteController noteController)
    {
        HandleNoteControllerNoteWasMissed(noteController);
    }
}