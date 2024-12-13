using CustomNotes.Models;
using CustomNotes.Utilities;
using SiraUtil.Objects;
using UnityEngine;
using Zenject;

namespace CustomNotes.Components;

internal class CustomBombController : MonoBehaviour, INoteControllerDidInitEvent, INoteControllerNoteWasCutEvent, INoteControllerNoteWasMissedEvent, INoteControllerNoteDidDissolveEvent
{
    private PluginConfig config;

    private BombNoteController bombNoteController;

    protected MeshRenderer VanillaBombRenderer;
    protected MeshRenderer FakeBombRenderer;

    protected SiraPrefabContainer SiraContainer;
    protected SiraPrefabContainer.Pool BombPool;

    [Inject]
    internal void Init(PluginConfig config, [InjectOptional(Id = Protocol.BombPool)] SiraPrefabContainer.Pool bombContainerPool)
    {
        this.config = config;

        name = "CustomBombNote";
            
        BombPool = bombContainerPool;

        bombNoteController = GetComponent<BombNoteController>();
        GetComponent<NoteMovement>();
    
        VanillaBombRenderer = gameObject.transform.Find("Mesh").GetComponent<MeshRenderer>();

        if (BombPool != null)
        {
            bombNoteController.didInitEvent.Add(this);
            bombNoteController.noteWasCutEvent.Add(this);
            bombNoteController.noteWasMissedEvent.Add(this);
            bombNoteController.noteDidDissolveEvent.Add(this);
            
            VanillaBombRenderer.enabled = false;
        }
            
        if (config.UseHmdOnly())
        {
            // create fake bombs because for some reason changing the layer of the vanilla bomb mesh causes them
            // to be unable to be cut.
            // TODO: investigate better solutions for the above ^
            FakeBombRenderer = Instantiate(VanillaBombRenderer, VanillaBombRenderer.transform, true);
            FakeBombRenderer.gameObject.name = "FakeCameraOnlyBomb";
            FakeBombRenderer.gameObject.layer = (int)NoteLayer.ThirdPerson;
            FakeBombRenderer.enabled = true;

            FakeBombRenderer.transform.localScale = Vector3.one;
            FakeBombRenderer.transform.localPosition = Vector3.zero;
            FakeBombRenderer.transform.rotation = Quaternion.identity;
        }
    }

    private void DidFinish()
    {
        SiraContainer.Prefab.SetActive(false);
        SiraContainer.transform.SetParent(null);
        BombPool.Despawn(SiraContainer);
    }

    public void HandleNoteControllerDidInit(NoteControllerBase noteController)
    {
        SiraContainer = BombPool.Spawn();
        
        var activeNoteBomb = SiraContainer.Prefab;
        activeNoteBomb.SetLayerRecursively(config.UseHmdOnly() ? NoteLayer.FirstPerson : NoteLayer.Note);
        activeNoteBomb.transform.localPosition = Vector3.zero;
        activeNoteBomb.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f) * config.GetNoteSize();
        activeNoteBomb.SetActive(true);
            
        SiraContainer.transform.SetParent(VanillaBombRenderer.transform);
        SiraContainer.transform.localPosition = Vector3.zero;
        SiraContainer.transform.localRotation = Quaternion.identity;
        SiraContainer.transform.localScale = Vector3.one;
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
        Destroy(FakeBombRenderer);
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