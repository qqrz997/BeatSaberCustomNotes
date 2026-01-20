using System.Collections.Generic;
using System.Linq;
using CameraUtils.Core;
using CustomNotes.Utilities;
using UnityEngine;

namespace CustomNotes.Components;

internal class CustomNoteColorNoteVisuals : ColorNoteVisuals
{
    private MeshRenderer[] ArrowRenderers => _arrowMeshRenderers.Concat(_circleMeshRenderers).ToArray();
    private readonly List<GameObject> duplicatedArrows = [];

    public void SetColor(Color color, bool updateMaterialBlocks)
    {
        _noteColor = color;
        if (updateMaterialBlocks)
        {
            foreach (var materialPropertyBlockController in _materialPropertyBlockControllers)
            {
                materialPropertyBlockController.materialPropertyBlock.SetColor(MaterialProps.Color, color with { a = 1f });
                materialPropertyBlockController.ApplyChanges();
            }
        }
    }

    public void TurnOffVisuals()
    {
        foreach (var arrowRenderer in ArrowRenderers)
        {
            arrowRenderer.enabled = false;
        }
    }

    public void SetBaseGameVisualsLayer(VisibilityLayer layer)
    {
        foreach (var arrowRenderer in ArrowRenderers)
        {
            arrowRenderer.gameObject.layer = (int)layer;
        }
    }

    public void CreateFakeVisuals(VisibilityLayer layer)
    {
        ClearDuplicatedArrows();
        foreach (var arrowRenderer in ArrowRenderers)
        {
            DuplicateIfExists(arrowRenderer.gameObject, layer);
        }
    }

    public void CreateAndScaleFakeVisuals(VisibilityLayer layer, float scale)
    {
        ClearDuplicatedArrows();
        foreach (var arrowRenderer in _arrowMeshRenderers)
        {
            ScaleIfExists(arrowRenderer.gameObject, layer, scale, new(0, 0.1f, -0.3f));
        }
        foreach (var circleRenderer in _circleMeshRenderers)
        {
            ScaleIfExists(circleRenderer.gameObject, layer, scale, new(0, 0, -0.25f));
        }
    }

    public void ScaleVisuals(float scale)
    {
        var scaleVector = new Vector3(1, 1, 1) * scale;

        foreach (var arrowRenderer in _arrowMeshRenderers)
        {
            if (arrowRenderer.gameObject.name == "NoteArrowGlow") arrowRenderer.gameObject.transform.localScale = new Vector3(0.6f, 0.3f, 0.6f) * scale;
            else arrowRenderer.gameObject.transform.localScale = scaleVector;

            arrowRenderer.gameObject.transform.localPosition = new Vector3(0, 0.1f, -0.3f) * scale;
        }

        foreach (var circleRenderer in _circleMeshRenderers)
        {
            circleRenderer.gameObject.transform.localScale = scaleVector / 2;
            circleRenderer.gameObject.transform.localPosition = new Vector3(0, 0, -0.3f) * scale;
        }
    }

    private void ClearDuplicatedArrows()
    {
        foreach (var arrow in duplicatedArrows)
        {
            arrow.SetActive(false);
            Destroy(arrow);
        }
        duplicatedArrows.Clear();
    }

    private GameObject DuplicateIfExists(GameObject gameObject, VisibilityLayer layer)
    {
        if (!gameObject.activeInHierarchy)
        {
            return null;
        }

        var tempObject = Instantiate(gameObject, gameObject.transform.parent, true);
        tempObject.transform.localScale = gameObject.transform.localScale;
        tempObject.transform.localPosition = gameObject.transform.localPosition;
        tempObject.SetLayerRecursively(layer);
        duplicatedArrows.Add(tempObject);
        return tempObject;
    }

    private void ScaleIfExists(GameObject gameObject, VisibilityLayer layer, float scale, Vector3 positionModifier)
    {
        var tempObject = DuplicateIfExists(gameObject, layer);
        if (tempObject != null)
        {
            var scaleVector = new Vector3(1, 1, 1) * scale;
            tempObject.transform.localScale = scaleVector;
            tempObject.transform.localPosition = positionModifier * scale;
        }
    }
}