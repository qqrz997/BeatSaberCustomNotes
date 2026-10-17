using AssetComponents.Components;
using CustomNotes.Utilities;
using UnityEngine;

namespace CustomNotes.Models;

internal class Note2Instance : MonoBehaviour, INote
{
    private MaterialColorer[] allColorers = [];
    
    public static Note2Instance AddTo(GameObject noteObject)
    {
        var instance = noteObject.AddComponent<Note2Instance>();
        instance.allColorers = noteObject.GetComponentsInChildren<MaterialColorer>(true);
        return instance;
    }

    public GameObject GameObject => gameObject;
    public Transform Transform => transform;

    public bool UseBaseArrows { get; } = false;
    public float ColorMultiplier { get; } = 1f;
    
    public void SetColorScheme(ColorScheme colorScheme)
    {
        foreach (var materialColorer in allColorers)
            colorScheme.ColorForType(materialColorer.ColorSchemeType);
    }

    public void SetBoostColors(ColorScheme colorScheme, bool boost)
    {
        foreach (var materialColorer in allColorers)
            colorScheme.BoostColorForType(materialColorer.ColorSchemeType, boost);
    }

    public void SetColor(Color color)
    {
        foreach (var materialColorer in allColorers)
            materialColorer.SetColor(color);
    }

    public void SetScale(float scaleMultiplier)
    {
        transform.localScale = Vector3.one * scaleMultiplier;
    }
}