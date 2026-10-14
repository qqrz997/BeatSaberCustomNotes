using CameraUtils.Core;
using CustomNotes.Models;
using CustomNotes.Utilities;
using SiraUtil.Interfaces;
using UnityEngine;
using Zenject;

namespace CustomNotes.Components;

internal class CustomBurstSliderController : MonoBehaviour, IColorable, INoteControllerNoteWasCutEvent, INoteControllerNoteWasMissedEvent, INoteControllerDidInitEvent, INoteControllerNoteDidDissolveEvent
{
    private PluginConfig config = null!;

    private Transform noteCube = null!;
    private CustomNoteData customNote = null!;
    private BurstSliderGameNoteController burstSliderGameNoteController = null!;
    private CustomNoteColorNoteVisuals customNoteColorNoteVisuals = null!;

    private INote? activeNote;
    private NoteInstanceContainer? activeContainer;
    private NoteInstanceContainer.Pool? activeSliderPool;
    private NoteInstanceContainer.Pool leftBurstSliderPool = null!;
    private NoteInstanceContainer.Pool rightBurstSliderPool = null!;

    [Inject]
    internal void Init(PluginConfig config,
        CustomNoteData customNote,
        [Inject(Id = Protocol.LeftBurstSliderPool)] NoteInstanceContainer.Pool leftBurstSliderPool,
        [Inject(Id = Protocol.RightBurstSliderPool)] NoteInstanceContainer.Pool rightBurstSliderPool)
    {
        this.config = config;
        this.customNote = customNote;

        this.leftBurstSliderPool = leftBurstSliderPool;
        this.rightBurstSliderPool = rightBurstSliderPool;

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
            noteMesh.gameObject.SetLayer(VisibilityLayer.DesktopOnlyAndReflected);
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
        if (activeContainer == null) return;
        activeContainer.transform.SetParent(null);

        if (noteController.noteData.colorType != ColorType.None)
        {
            activeContainer.gameObject.SetActive(false);
            activeSliderPool?.Despawn(activeContainer);
            activeContainer = null;
        }
    }

    public void HandleNoteControllerDidInit(NoteControllerBase noteController)
    {
        activeSliderPool = noteController.noteData.colorType == ColorType.ColorA ? leftBurstSliderPool : rightBurstSliderPool;
        activeContainer = activeSliderPool.Spawn();
        if (!activeContainer) return;
        activeNote = activeContainer.Instance;
        
        var go = activeNote.GameObject;
        go.SetLayerRecursively(config.HmdOnly ? VisibilityLayer.HmdOnlyAndReflected : VisibilityLayer.Note);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f) * config.GetNoteSize();
        go.gameObject.SetActive(true);
            
        activeContainer.transform.SetParent(noteCube);
        activeContainer.transform.localRotation = Quaternion.identity;
        activeContainer.transform.localScale = Vector3.one;
        activeContainer.transform.localPosition = Vector3.zero;
    }

    private void SetActiveThenColor(INote note, Color color)
    {
        note.GameObject.SetActive(true);
        if (customNote.Prefab is { Colorable: true })
        {
            SetColor(color, true);
        }
    }

    private void Visuals_DidInit(ColorNoteVisuals visuals, NoteControllerBase noteController)
    {
        if (activeNote == null) return;
        SetActiveThenColor(activeNote, ((CustomNoteColorNoteVisuals)visuals)._noteColor);
            
        // Hide certain parts of the default note which is not required
        if (!config.HmdOnly)
        {
            customNoteColorNoteVisuals.SetBaseGameVisualsLayer(VisibilityLayer.Note);
            if (!activeNote.UseBaseArrows)
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
        customNoteColorNoteVisuals.SetBaseGameVisualsLayer(VisibilityLayer.DesktopOnlyAndReflected);
        if (activeNote.UseBaseArrows)
        {
            if (!config.NoteSizeEquals(1))
            {
                // arrows should be enabled in both views, with fake arrows rescaled
                customNoteColorNoteVisuals.CreateAndScaleFakeVisuals(VisibilityLayer.HmdOnlyAndReflected, config.GetNoteSize());
            }
            else
            {
                // arrows should be enabled in both views
                customNoteColorNoteVisuals.CreateFakeVisuals(VisibilityLayer.HmdOnlyAndReflected);
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
        if (activeNote == null) return;
        customNoteColorNoteVisuals.SetColor(color, updateMatBlocks);
        activeNote.SetColor(color * activeNote.ColorMultiplier);
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