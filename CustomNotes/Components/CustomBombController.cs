using CameraUtils.Core;
using CustomNotes.Models;
using CustomNotes.Utilities;
using UnityEngine;
using Zenject;

namespace CustomNotes.Components;

internal class CustomBombController : MonoBehaviour, INoteControllerDidInitEvent, INoteControllerNoteWasCutEvent, INoteControllerNoteWasMissedEvent, INoteControllerNoteDidDissolveEvent
{
    private PluginConfig config = null!;
    private BombNoteController bombNoteController = null!;
    
    private MeshRenderer vanillaBombRenderer = null!;
    private MeshRenderer fakeBombRenderer = null!;

    private INote? activeNote;
    private NoteInstanceContainer? activeContainer;
    private NoteInstanceContainer.Pool bombPool = null!;

    [Inject]
    public void Init(
        PluginConfig config, 
        [Inject(Id = Protocol.BombPool)] NoteInstanceContainer.Pool bombContainerPool)
    {
        this.config = config;

        name = "CustomBombNote";
            
        bombPool = bombContainerPool;

        bombNoteController = GetComponent<BombNoteController>();
        GetComponent<NoteMovement>();
    
        vanillaBombRenderer = gameObject.transform.Find("Mesh").GetComponent<MeshRenderer>();

        bombNoteController.didInitEvent.Add(this);
        bombNoteController.noteWasCutEvent.Add(this);
        bombNoteController.noteWasMissedEvent.Add(this);
        bombNoteController.noteDidDissolveEvent.Add(this);
            
        vanillaBombRenderer.enabled = false;

        if (config.HmdOnly)
        {
            // create fake bombs because for some reason changing the layer of the vanilla bomb mesh causes them
            // to be unable to be cut.
            // TODO: investigate better solutions for the above ^
            fakeBombRenderer = Instantiate(vanillaBombRenderer, vanillaBombRenderer.transform, true);
            fakeBombRenderer.gameObject.name = "FakeCameraOnlyBomb";
            fakeBombRenderer.gameObject.SetLayer(VisibilityLayer.DesktopOnlyAndReflected);
            fakeBombRenderer.enabled = true;

            fakeBombRenderer.transform.localScale = Vector3.one;
            fakeBombRenderer.transform.localPosition = Vector3.zero;
            fakeBombRenderer.transform.rotation = Quaternion.identity;
        }
    }

    private void DidFinish()
    {
        if (!activeContainer) return;
        activeNote?.GameObject.SetActive(false);
        activeContainer.transform.SetParent(null);
        bombPool.Despawn(activeContainer);
    }

    public void HandleNoteControllerDidInit(NoteControllerBase noteController)
    {
        activeContainer = bombPool.Spawn();
        if (!activeContainer) return;
        activeNote = activeContainer.Instance;
        
        var go = activeNote.GameObject;
        go.SetLayerRecursively(config.HmdOnly ? VisibilityLayer.HmdOnlyAndReflected : VisibilityLayer.Note);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f) * config.GetNoteSize();
        go.SetActive(true);
            
        activeContainer.transform.SetParent(vanillaBombRenderer.transform);
        activeContainer.transform.localPosition = Vector3.zero;
        activeContainer.transform.localRotation = Quaternion.identity;
        activeContainer.transform.localScale = Vector3.one;
    }

    protected void OnDestroy()
    {
        if (bombNoteController != null)
        {
            bombNoteController.didInitEvent.Remove(this);
            bombNoteController.noteWasCutEvent.Remove(this);
            bombNoteController.noteWasMissedEvent.Remove(this);
            bombNoteController.noteDidDissolveEvent.Remove(this);
        }
        Destroy(fakeBombRenderer);
    }

    public void HandleNoteControllerNoteDidDissolve(NoteController _)
    {
        DidFinish();
    }

    public void HandleNoteControllerNoteWasCut(NoteController _, in NoteCutInfo __)
    {
        DidFinish();
    }

    public void HandleNoteControllerNoteWasMissed(NoteController _)
    {
        DidFinish();
    }
}