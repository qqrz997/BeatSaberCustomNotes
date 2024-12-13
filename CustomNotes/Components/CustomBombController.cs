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

    protected MeshRenderer VanillaBombMesh;
    protected GameObject FakeFirstPersonBombMesh;

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

        if (BombPool != null)
        {
            bombNoteController.didInitEvent.Add(this);
            bombNoteController.noteWasCutEvent.Add(this);
            bombNoteController.noteWasMissedEvent.Add(this);
            bombNoteController.noteDidDissolveEvent.Add(this);
        }
    
        VanillaBombMesh = gameObject.transform.Find("Mesh").GetComponent<MeshRenderer>();
            
        if (config.UseHmdOnly())
        {
            // create fake bombs because for some reason changing the layer of the vanilla bomb mesh causes them
            // to be unable to be cut.
            // TODO: investigate better solutions for the above ^
            FakeFirstPersonBombMesh = Instantiate(VanillaBombMesh.gameObject, VanillaBombMesh.transform, true);
            FakeFirstPersonBombMesh.name = "FakeFirstPersonBomb";

            FakeFirstPersonBombMesh.transform.localScale = Vector3.one;
            FakeFirstPersonBombMesh.transform.localPosition = Vector3.zero;
            FakeFirstPersonBombMesh.transform.rotation = Quaternion.identity;
            FakeFirstPersonBombMesh.layer = (int)NoteLayer.FirstPerson;
        }
        else if (BombPool != null)
        {
            VanillaBombMesh.enabled = false;
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
            
        SiraContainer.transform.SetParent(VanillaBombMesh.transform);
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
        Destroy(FakeFirstPersonBombMesh);
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