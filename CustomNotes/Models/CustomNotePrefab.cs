using UnityEngine;

namespace CustomNotes.Models;

internal class CustomNotePrefab
{
    public CustomNotePrefab(
        NotePrefabs left,
        NotePrefabs right,
        INotePrefab bomb, 
        bool colorableNote = true,
        float colorMultiplier = 1f)
    {
        Left = left;
        Right = right;
        Bomb = bomb;
        Colorable = colorableNote;
        ColorMultiplier = colorMultiplier;
    }

    public NotePrefabs Left { get; }
    public NotePrefabs Right { get; }
    public INotePrefab Bomb { get; }
    
    public bool Colorable { get; }
    public float ColorMultiplier { get; }
}