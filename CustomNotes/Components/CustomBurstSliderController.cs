using CustomNotes.Managers;
using CustomNotes.Models;
using CustomNotes.Utilities;
using SiraUtil.Interfaces;
using SiraUtil.Objects;
using UnityEngine;
using Zenject;

namespace CustomNotes.Components;

internal class CustomBurstSliderController : MonoBehaviour, IColorable, INoteControllerNoteWasCutEvent, INoteControllerNoteWasMissedEvent, INoteControllerDidInitEvent, INoteControllerNoteDidDissolveEvent
{
    private PluginConfig config = null!;

    private Transform noteCube = null!;
    private CustomNote customNote = null!;
    private BurstSliderGameNoteController burstSliderGameNoteController = null!;
    private CustomNoteColorNoteVisuals customNoteColorNoteVisuals = null!;

    private GameObject activeNote;
    private SiraPrefabContainer siraContainer;
    private SiraPrefabContainer.Pool activeSliderPool;
    private SiraPrefabContainer.Pool leftBurstSliderPool;
    private SiraPrefabContainer.Pool rightBurstSliderPool;

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

        noteCube = burstSliderGameNoteController.gameObject.transform.Find("NoteCube");

        var noteMesh = GetComponentInChildren<MeshRenderer>();
        if (config.HmdOnly)
        {
            noteMesh.gameObject.layer = (int)NoteLayer.ThirdPerson;
        }
        else
        {
            // only disable if custom notes display on both hmd and display
            noteMesh.forceRenderingOff = true;
        }
    }

    public Color Color
    {
        get => customNoteColorNoteVisuals != null ? customNoteColorNoteVisuals._noteColor : Color.white;
        set => SetColor(value);
    }

    public void HandleNoteControllerNoteWasMissed(NoteController noteController)
    {
        if (siraContainer != null)
        {
            siraContainer.transform.SetParent(null);

            if (noteController.noteData.colorType != ColorType.None)
            {
                siraContainer.Prefab.SetActive(false);
                activeSliderPool?.Despawn(siraContainer);
                siraContainer = null;
            }
        }
    }

    public void HandleNoteControllerDidInit(NoteControllerBase noteController)
    {
        activeSliderPool = noteController.noteData.colorType == ColorType.ColorA ? leftBurstSliderPool : rightBurstSliderPool;
        siraContainer = activeSliderPool.Spawn();

        activeNote = siraContainer.Prefab;
        activeNote.SetLayerRecursively(config.HmdOnly ? NoteLayer.FirstPerson : NoteLayer.Note);
        activeNote.transform.localPosition = Vector3.zero;
        activeNote.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f) * config.GetNoteSize();
        activeNote.SetActive(true);
            
        siraContainer.transform.SetParent(noteCube);
        siraContainer.transform.localRotation = Quaternion.identity;
        siraContainer.transform.localScale = Vector3.one;
        siraContainer.transform.localPosition = Vector3.zero;
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
        SetActiveThenColor(activeNote, ((CustomNoteColorNoteVisuals)visuals)._noteColor);
            
        // Hide certain parts of the default note which is not required
        if (!config.HmdOnly)
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
        if (activeNote != null)
        {
            customNoteColorNoteVisuals.SetColor(color, updateMatBlocks);
            Utils.ColorizeCustomNote(color, customNote.Descriptor.NoteColorStrength, activeNote);
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