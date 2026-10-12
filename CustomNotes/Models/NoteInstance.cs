using System.Collections.Generic;
using System.Linq;
using CustomNotes.Utilities;
using UnityEngine;

namespace CustomNotes.Models;

internal class NoteInstance : MonoBehaviour
{
    [SerializeField] private Renderer[] colorableRenderers = [];
    [SerializeField] private bool useBaseArrows;
    [SerializeField] private float colorMultiplier;
    
    private readonly MaterialPropertyBlock materialPropertyBlock = new();
    
    public static NoteInstance AddTo(GameObject noteObject, bool useBaseArrows, float colorMultiplier)
    {
        var noteInstance = noteObject.AddComponent<NoteInstance>();
        noteInstance.colorableRenderers = GetColorableRenderers(noteObject).ToArray();
        noteInstance.useBaseArrows = useBaseArrows;
        noteInstance.colorMultiplier = colorMultiplier;
        return noteInstance;
    }
    private MaterialPropertyBlockController? materialPropertyBlockController;

    public bool UseBaseArrows => useBaseArrows;
    public float ColorMultiplier => colorMultiplier;

    public void SetColorScheme(ColorScheme colorScheme)
    {
    }

    public void SetBoostColors(ColorScheme colorScheme, bool boost)
    {
    }

    public void OnDestroy()
    {
        if (gameObject) Destroy(gameObject);
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