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
        if (noteModelPrefab == null)
            return null;

        var gameObject = Object.Instantiate(noteModelPrefab);
        MaterialSwapper.ReplaceMaterialsForGameObject(gameObject);
        return NoteInstance.AddTo(gameObject, useBaseArrows, colorMultiplier);
    }
}