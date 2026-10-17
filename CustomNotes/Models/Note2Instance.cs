using System.Linq;
using AssetComponents.Components;
using CustomNotes.Utilities;
using UnityEngine;

namespace CustomNotes.Models;

internal class Note2Instance : MonoBehaviour, INote
{
    [SerializeField] private MaterialColorer[] allColorers = [];
    [SerializeField] private MaterialColorer[] boostColors = [];
    [SerializeField] private MaterialColorer[] saberColors = [];
    
    public static Note2Instance AddTo(GameObject noteObject)
    {
        var instance = noteObject.AddComponent<Note2Instance>();
        instance.allColorers = noteObject.GetComponentsInChildren<MaterialColorer>(true);
        instance.boostColors = instance.allColorers.Where(c => c.UsesBoostColors()).ToArray();
        instance.saberColors = instance.allColorers.Where(c => c.UsesSaberColors()).ToArray();
        return instance;
    }

    public GameObject GameObject => gameObject;
    public Transform Transform => transform;

    public bool UseBaseArrows { get; } = false;
    public float ColorMultiplier { get; } = 1f;
    
    public void SetColorScheme(ColorScheme colorScheme)
    {
        foreach (var materialColorer in allColorers)
        {
            var color = colorScheme.GetColorByType(materialColorer.ColorSchemeType);
            materialColorer.SetColor(color);
        }
    }

    public void SetBoostColors(ColorScheme colorScheme, bool boost)
    {
        foreach (var materialColorer in boostColors)
        {
            var color = colorScheme.GetBoostColorByType(materialColorer.ColorSchemeType, boost);
            materialColorer.SetColor(color);
        }
    }

    public void SetColor(Color color)
    {
        foreach (var materialColorer in saberColors)
        {
            materialColorer.SetColor(color);
        }
    }

    public void SetScale(float scaleMultiplier)
    {
        transform.localScale = Vector3.one * scaleMultiplier;
    }
}