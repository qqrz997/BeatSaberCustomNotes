﻿using CustomNotes.Models;
using CustomNotes.Utilities;
using SiraUtil.Objects;
using UnityEngine;
using Zenject;

namespace CustomNotes.Components;

internal class CustomBombController : MonoBehaviour, INoteControllerDidInitEvent, INoteControllerNoteWasCutEvent, INoteControllerNoteWasMissedEvent, INoteControllerNoteDidDissolveEvent
{
    private PluginConfig config = null!;
    private BombNoteController bombNoteController = null!;
    
    private MeshRenderer vanillaBombRenderer = null!;
    private MeshRenderer fakeBombRenderer;

    private SiraPrefabContainer siraContainer;
    private SiraPrefabContainer.Pool bombPool;

    [Inject]
    private void Init(
        PluginConfig config, 
        [InjectOptional(Id = Protocol.BombPool)] SiraPrefabContainer.Pool bombContainerPool)
    {
        this.config = config;

        name = "CustomBombNote";
            
        bombPool = bombContainerPool;

        bombNoteController = GetComponent<BombNoteController>();
        GetComponent<NoteMovement>();
    
        vanillaBombRenderer = gameObject.transform.Find("Mesh").GetComponent<MeshRenderer>();

        if (bombPool != null)
        {
            bombNoteController.didInitEvent.Add(this);
            bombNoteController.noteWasCutEvent.Add(this);
            bombNoteController.noteWasMissedEvent.Add(this);
            bombNoteController.noteDidDissolveEvent.Add(this);
            
            vanillaBombRenderer.enabled = false;
        }
            
        if (config.HmdOnly)
        {
            // create fake bombs because for some reason changing the layer of the vanilla bomb mesh causes them
            // to be unable to be cut.
            // TODO: investigate better solutions for the above ^
            fakeBombRenderer = Instantiate(vanillaBombRenderer, vanillaBombRenderer.transform, true);
            fakeBombRenderer.gameObject.name = "FakeCameraOnlyBomb";
            fakeBombRenderer.gameObject.layer = (int)NoteLayer.ThirdPerson;
            fakeBombRenderer.enabled = true;

            fakeBombRenderer.transform.localScale = Vector3.one;
            fakeBombRenderer.transform.localPosition = Vector3.zero;
            fakeBombRenderer.transform.rotation = Quaternion.identity;
        }
    }

    private void DidFinish()
    {
        siraContainer.Prefab.SetActive(false);
        siraContainer.transform.SetParent(null);
        bombPool.Despawn(siraContainer);
    }

    public void HandleNoteControllerDidInit(NoteControllerBase noteController)
    {
        siraContainer = bombPool.Spawn();
        
        var activeNoteBomb = siraContainer.Prefab;
        activeNoteBomb.SetLayerRecursively(config.HmdOnly ? NoteLayer.FirstPerson : NoteLayer.Note);
        activeNoteBomb.transform.localPosition = Vector3.zero;
        activeNoteBomb.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f) * config.GetNoteSize();
        activeNoteBomb.SetActive(true);
            
        siraContainer.transform.SetParent(vanillaBombRenderer.transform);
        siraContainer.transform.localPosition = Vector3.zero;
        siraContainer.transform.localRotation = Quaternion.identity;
        siraContainer.transform.localScale = Vector3.one;
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