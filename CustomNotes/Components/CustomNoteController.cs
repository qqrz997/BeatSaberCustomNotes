using CameraUtils.Core;
using CustomNotes.Models;
using CustomNotes.Utilities;
using SiraUtil.Interfaces;
using UnityEngine;
using Zenject;

namespace CustomNotes.Components;

internal class CustomNoteController : MonoBehaviour, IColorable, INoteControllerNoteWasCutEvent, INoteControllerNoteWasMissedEvent, INoteControllerDidInitEvent, INoteControllerNoteDidDissolveEvent
{
    private PluginConfig config = null!;
    private CustomNoteData customNote = null!;

    private Transform noteCube = null!;
    private GameNoteController gameNoteController = null!;
    private CustomNoteColorNoteVisuals customNoteColorNoteVisuals = null!;

    private INote? activeNote;
    private NoteInstanceContainer? activeContainer;
    private NoteInstanceContainer.Pool? activePool;

    private NoteInstanceContainer.Pool? leftDotNotePool;
    private NoteInstanceContainer.Pool? rightDotNotePool;
    private NoteInstanceContainer.Pool leftArrowNotePool = null!;
    private NoteInstanceContainer.Pool rightArrowNotePool = null!;
    private NoteInstanceContainer.Pool leftBurstSliderHeadPool = null!;
    private NoteInstanceContainer.Pool rightBurstSliderHeadPool = null!;
    private NoteInstanceContainer.Pool leftBurstSliderHeadDotPool = null!;
    private NoteInstanceContainer.Pool rightBurstSliderHeadDotPool = null!;

    public Color Color
    {
        get => customNoteColorNoteVisuals != null ? customNoteColorNoteVisuals._noteColor : Color.white;
        set => SetColor(value);
    }

    [Inject]
    internal void Init(
        PluginConfig config,
        CustomNoteData customNote,
        [Inject(Id = Protocol.LeftArrowPool)] NoteInstanceContainer.Pool leftArrowNotePool,
        [Inject(Id = Protocol.RightArrowPool)] NoteInstanceContainer.Pool rightArrowNotePool,
        [InjectOptional(Id = Protocol.LeftDotPool)] NoteInstanceContainer.Pool? leftDotNotePool,
        [InjectOptional(Id = Protocol.RightDotPool)] NoteInstanceContainer.Pool? rightDotNotePool,
        [Inject(Id = Protocol.LeftBurstSliderHeadPool)] NoteInstanceContainer.Pool leftBurstSliderHeadPool,
        [Inject(Id = Protocol.RightBurstSliderHeadPool)] NoteInstanceContainer.Pool rightBurstSliderHeadPool,
        [Inject(Id = Protocol.LeftBurstSliderHeadDotPool)] NoteInstanceContainer.Pool leftBurstSliderHeadDotPool,
        [Inject(Id = Protocol.RightBurstSliderHeadDotPool)] NoteInstanceContainer.Pool rightBurstSliderHeadDotPool)
    {
        this.config = config;
        this.customNote = customNote;
        
        this.leftArrowNotePool = leftArrowNotePool;
        this.rightArrowNotePool = rightArrowNotePool;

        this.leftDotNotePool = leftDotNotePool ?? this.leftArrowNotePool;
        this.rightDotNotePool = rightDotNotePool ?? this.rightArrowNotePool;

        this.leftBurstSliderHeadPool = leftBurstSliderHeadPool;
        this.rightBurstSliderHeadPool = rightBurstSliderHeadPool;
        this.leftBurstSliderHeadDotPool = leftBurstSliderHeadDotPool;
        this.rightBurstSliderHeadDotPool = rightBurstSliderHeadDotPool;

        gameNoteController = GetComponent<GameNoteController>();
        customNoteColorNoteVisuals = gameObject.GetComponent<CustomNoteColorNoteVisuals>();
        customNoteColorNoteVisuals.enabled = true;

        gameNoteController.didInitEvent.Add(this);
        gameNoteController.noteWasCutEvent.Add(this);
        gameNoteController.noteWasMissedEvent.Add(this);
        gameNoteController.noteDidDissolveEvent.Add(this);
        customNoteColorNoteVisuals.didInitEvent += Visuals_DidInit;

        noteCube = gameNoteController.gameObject.transform.Find("NoteCube");

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

    public void HandleNoteControllerNoteWasMissed(NoteController noteController)
    {
        if (!activeContainer) return;
        activeContainer.transform.SetParent(null);
            
        if (noteController.noteData.colorType != ColorType.None)
        {
            activeContainer.gameObject.SetActive(false);
            activePool?.Despawn(activeContainer);
            activeContainer = null;
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

    private void SpawnThenParent(NoteInstanceContainer.Pool? noteModelPool)
    {
        activePool = noteModelPool;
        activeContainer = noteModelPool?.Spawn();
        if (!activeContainer) return;
        activeNote = activeContainer.Instance;

        activeNote.GameObject.SetLayerRecursively(config.HmdOnly ? VisibilityLayer.HmdOnlyAndReflected : VisibilityLayer.Note);
        activeNote.GameObject.transform.localPosition = Vector3.zero;
        activeNote.SetScale(config.GetNoteSize());
        activeNote.GameObject.SetActive(true);
        
        activeContainer.transform.SetParent(noteCube);
        activeContainer.transform.localPosition = Vector3.zero;
        activeContainer.transform.localRotation = Quaternion.identity;
        activeContainer.transform.localScale = Vector3.one;
    }

    private void Visuals_DidInit(ColorNoteVisuals visuals, NoteControllerBase noteController)
    {
        if (activeNote == null) return;
        activeNote.GameObject.SetActive(true);
        
        SetColor(visuals._noteColor, true);
        
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
        if (activeNote == null) return;
        if (customNote.Prefab is LegacyNotePrefab { Colorable: false }) return;
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