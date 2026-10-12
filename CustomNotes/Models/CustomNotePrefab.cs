using UnityEngine;

namespace CustomNotes.Models;

internal class CustomNotePrefab
{
    public CustomNotePrefab(
        NotePrefabs left,
        NotePrefabs right,
        NotePrefab bomb, 
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
    public NotePrefab Bomb { get; }
    
    public bool Colorable { get; }
    public float ColorMultiplier { get; }
}