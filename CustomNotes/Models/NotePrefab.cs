using CustomNotes.Utilities;
using UnityEngine;

namespace CustomNotes.Models;

internal class NotePrefab
{
    private readonly GameObject noteModelPrefab;
    private readonly bool useBaseArrows;
    private readonly float colorMultiplier;

    public NotePrefab(GameObject noteModelPrefab, 
        bool useBaseArrows = false,
        float colorMultiplier = 1f)
    {
        this.noteModelPrefab = noteModelPrefab;
        this.useBaseArrows = useBaseArrows;
        this.colorMultiplier = colorMultiplier;
    }

    public NoteInstance Instantiate()
    {
        var gameObject = Object.Instantiate(noteModelPrefab);
        gameObject.ReplaceSwappableMaterials();
        return NoteInstance.AddTo(gameObject, useBaseArrows, colorMultiplier);
    }
}