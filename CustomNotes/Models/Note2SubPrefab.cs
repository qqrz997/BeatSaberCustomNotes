using UnityEngine;

namespace CustomNotes.Models;

internal class Note2SubPrefab : INoteSubPrefab
{
    private readonly GameObject noteModelPrefab;

    public Note2SubPrefab(GameObject noteModelPrefab)
    {
        this.noteModelPrefab = noteModelPrefab;
    }
    
    public INote Instantiate()
    {
        var gameObject = Object.Instantiate(noteModelPrefab);
        return Note2Instance.AddTo(gameObject);
    }
}