using System.Collections.Generic;
using System.Linq;
using CustomNotes.Utilities;
using UnityEngine;

namespace CustomNotes.Models;

internal class LegacyNoteInstance : MonoBehaviour, INote
{
    [SerializeField] private Renderer[] colorableRenderers = [];
    [SerializeField] private bool useBaseArrows;
    [SerializeField] private float colorMultiplier;
    
    private readonly MaterialPropertyBlock materialPropertyBlock = new();
    
    public static LegacyNoteInstance AddTo(GameObject noteObject, bool useBaseArrows, float colorMultiplier)
    {
        var noteInstance = noteObject.AddComponent<LegacyNoteInstance>();
        noteInstance.colorableRenderers = GetColorableRenderers(noteObject).ToArray();
        noteInstance.useBaseArrows = useBaseArrows;
        noteInstance.colorMultiplier = colorMultiplier;
        return noteInstance;
    }
    
    private MaterialPropertyBlockController? materialPropertyBlockController;
    
    public GameObject GameObject => gameObject;
    public Transform Transform => transform;

    public bool UseBaseArrows => useBaseArrows;
    public float ColorMultiplier => colorMultiplier;

    public void SetColorScheme(ColorScheme colorScheme)
    {
    }

    public void SetBoostColors(ColorScheme colorScheme, bool boost)
    {
    }
    
    public void SetColor(Color color)
    {
        materialPropertyBlock.SetColor(MaterialProps.Color, color);

        foreach (var renderer in colorableRenderers)
        {
            renderer.SetPropertyBlock(materialPropertyBlock);
        }

        if (!materialPropertyBlockController)
        {
            materialPropertyBlockController = gameObject.GetComponent<MaterialPropertyBlockController>();
        }

        if (!materialPropertyBlockController) return;
        materialPropertyBlockController.materialPropertyBlock.SetColor(MaterialProps.Color, color);
        materialPropertyBlockController.ApplyChanges();
    }

    public void SetScale(float scaleMultiplier)
    {
        transform.localScale = Vector3.one * (0.4f * scaleMultiplier);
    }

    private void OnDestroy()
    {
        if (gameObject) Destroy(gameObject);
    }

    private static IEnumerable<Renderer> GetColorableRenderers(GameObject gameObject)
    {
        var renderers = gameObject.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            if (renderer.sharedMaterial.HasColor(MaterialProps.Color)
                && !renderer.GetComponent<DisableNoteColorOnGameobject>())
            {
                yield return renderer;
            }
        }
    }
}