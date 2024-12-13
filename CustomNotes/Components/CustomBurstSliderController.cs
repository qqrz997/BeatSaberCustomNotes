﻿using CustomNotes.Managers;
using CustomNotes.Models;
using CustomNotes.Utilities;
using SiraUtil.Interfaces;
using SiraUtil.Objects;
using UnityEngine;
using Zenject;

namespace CustomNotes.Components;

public class CustomBurstSliderController : MonoBehaviour, IColorable, INoteControllerNoteWasCutEvent, INoteControllerNoteWasMissedEvent, INoteControllerDidInitEvent, INoteControllerNoteDidDissolveEvent
{
    private PluginConfig config;

    protected Transform NoteCube;
    private CustomNote customNote;
    private BurstSliderGameNoteController burstSliderGameNoteController;
    private CustomNoteColorNoteVisuals customNoteColorNoteVisuals;

    protected GameObject ActiveNote;
    protected SiraPrefabContainer Container;
    protected SiraPrefabContainer.Pool ActivePool;

    private SiraPrefabContainer.Pool leftBurstSliderPool;
    private SiraPrefabContainer.Pool rightBurstSliderPool;

    public Color Color
    {
        get => customNoteColorNoteVisuals != null ? customNoteColorNoteVisuals._noteColor : Color.white;
        set => SetColor(value);
    }

    [Inject]
    internal void Init(PluginConfig config,
        NoteAssetLoader noteAssetLoader,
        [Inject(Id = Protocol.LeftBurstSliderPool)] SiraPrefabContainer.Pool leftBurstSliderPool,
        [Inject(Id = Protocol.RightBurstSliderPool)] SiraPrefabContainer.Pool rightBurstSliderPool)
    {
        this.config = config;
        this.leftBurstSliderPool = leftBurstSliderPool;
        this.rightBurstSliderPool = rightBurstSliderPool;

        customNote = noteAssetLoader.CustomNoteObjects[noteAssetLoader.SelectedNoteIdx];

        customNoteColorNoteVisuals = gameObject.GetComponent<CustomNoteColorNoteVisuals>();
        customNoteColorNoteVisuals.enabled = true;
        customNoteColorNoteVisuals.didInitEvent += Visuals_DidInit;

        burstSliderGameNoteController = GetComponent<BurstSliderGameNoteController>();
        burstSliderGameNoteController.didInitEvent.Add(this);
        burstSliderGameNoteController.noteWasCutEvent.Add(this);
        burstSliderGameNoteController.noteWasMissedEvent.Add(this);
        burstSliderGameNoteController.noteDidDissolveEvent.Add(this);

        NoteCube = burstSliderGameNoteController.gameObject.transform.Find("NoteCube");

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
        ActivePool = noteController.noteData.colorType == ColorType.ColorA ? leftBurstSliderPool : rightBurstSliderPool;
        Container = ActivePool.Spawn();

        ActiveNote = Container.Prefab;
        ActiveNote.SetLayerRecursively(config.UseHmdOnly() ? NoteLayer.FirstPerson : NoteLayer.Note);
        ActiveNote.transform.localPosition = Vector3.zero;
        ActiveNote.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f) * config.GetNoteSize();
        ActiveNote.SetActive(true);
            
        Container.transform.SetParent(NoteCube);
        Container.transform.localRotation = Quaternion.identity;
        Container.transform.localScale = Vector3.one;
        Container.transform.localPosition = Vector3.zero;
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
        if (burstSliderGameNoteController != null)
        {
            burstSliderGameNoteController.didInitEvent.Remove(this);
            burstSliderGameNoteController.noteWasCutEvent.Remove(this);
            burstSliderGameNoteController.noteWasMissedEvent.Remove(this);
            burstSliderGameNoteController.noteDidDissolveEvent.Remove(this);
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