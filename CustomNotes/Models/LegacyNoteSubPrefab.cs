using CustomNotes.Utilities;
using UnityEngine;

namespace CustomNotes.Models;

internal class LegacyNoteSubPrefab : INoteSubPrefab
{
    private readonly GameObject noteModelPrefab;
    private readonly bool useBaseArrows;
    private readonly float colorMultiplier;

    public LegacyNoteSubPrefab(GameObject noteModelPrefab, 
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
        return LegacyNoteInstance.AddTo(gameObject, useBaseArrows, colorMultiplier);
    }
}