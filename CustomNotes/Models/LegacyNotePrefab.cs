using AssetComponents.Components;
using CustomNotes.Utilities;
using UnityEngine;

namespace CustomNotes.Models;

internal class LegacyNotePrefab : INotePrefab
{
    private readonly GameObject noteModelPrefab;
    private readonly bool useBaseArrows;
    private readonly float colorMultiplier;

    public LegacyNotePrefab(GameObject noteModelPrefab, 
        bool useBaseArrows = false,
        float colorMultiplier = 1f)
    {
        this.noteModelPrefab = noteModelPrefab;
        this.useBaseArrows = useBaseArrows;
        this.colorMultiplier = colorMultiplier;
    }

    public INote Instantiate()
    {
        var gameObject = Object.Instantiate(noteModelPrefab);
        gameObject.ReplaceSwappableMaterials();
        return NoteInstance.AddTo(gameObject, useBaseArrows, colorMultiplier);
    }
}

internal class Note2Prefab : INotePrefab
{
    private readonly GameObject noteModelPrefab;

    public Note2Prefab(GameObject noteModelPrefab)
    {
        this.noteModelPrefab = noteModelPrefab;
    }
    
    public INote Instantiate()
    {
        var gameObject = Object.Instantiate(noteModelPrefab);
        return Note2Instance.AddTo(gameObject);
    }
}

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
}