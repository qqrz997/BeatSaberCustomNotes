using CustomNotes.Utilities;
using UnityEngine;

namespace CustomNotes.Models;

internal class DefaultNoteModel : INoteModel
{
    private readonly MaterialPropertyBlockController[] materialPropertyBlockControllers;
    private readonly MeshRenderer noteRenderer;
    private readonly MeshRenderer[] arrowRenderers;
    private readonly MeshRenderer[] dotRenderers;
    
    public DefaultNoteModel(GameObject notePrefab, bool active)
    {
        var instance = Object.Instantiate(notePrefab);
        
        var noteCube = instance.transform.Find("NoteCube");
        noteCube.SetParent(null);
        noteCube.localPosition = Vector3.zero;

        var colorVisuals = instance.GetComponent<ColorNoteVisuals>();
        materialPropertyBlockControllers = colorVisuals._materialPropertyBlockControllers;
        noteRenderer = noteCube.GetComponent<MeshRenderer>();
        arrowRenderers = colorVisuals._arrowMeshRenderers;
        dotRenderers = colorVisuals._circleMeshRenderers;
        
        Object.Destroy(instance);
        Object.Destroy(noteCube.Find("BigCuttable").gameObject);
        Object.Destroy(noteCube.Find("SmallCuttable").gameObject);
        
        foreach (var monoBehaviour in noteCube.GetComponentsInChildren<MonoBehaviour>())
            monoBehaviour.enabled = false;

        GameObject = noteCube.gameObject;
        GameObject.SetActive(active);
        Transform = noteCube.transform;
    }

    private bool showDot;
    private bool showNoteMesh;

    public GameObject GameObject { get; }
    public Transform Transform { get; }

    public bool ShowDot
    {
        get => showDot;
        set => SetShowDot(value);
    }

    public bool ShowNoteMesh
    {
        get => showNoteMesh;
        set => SetNoteEnabled(value);
    }

    public void SetColor(Color color)
    {
        foreach (var materialPropertyBlockController in materialPropertyBlockControllers)
        {
            materialPropertyBlockController.materialPropertyBlock.SetColor(MaterialProps.Color, color);
            materialPropertyBlockController.ApplyChanges();
        }
    }

    private void SetShowDot(bool value)
    {
        showDot = value;
        foreach (var arrowRenderer in arrowRenderers) arrowRenderer.enabled = !showDot;
        foreach (var dotRenderer in dotRenderers) dotRenderer.enabled = showDot;
    }

    private void SetNoteEnabled(bool value)
    {
        showNoteMesh = value;
        noteRenderer.enabled = showNoteMesh;
    }
}