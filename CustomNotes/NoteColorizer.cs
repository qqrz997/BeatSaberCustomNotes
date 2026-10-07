using AssetComponents.Components;
using UnityEngine;

namespace CustomNotes.Installers;

internal class NoteColorizer : MonoBehaviour
{
    private MaterialColorer[] allColorers;

    private void Awake()
    {
        allColorers ??= GetComponentsInChildren<MaterialColorer>(true) ?? [];
    }
    
    public void SetColor(Color color)
    {
        foreach (var colorer in allColorers) colorer.SetColor(color);
    }
}