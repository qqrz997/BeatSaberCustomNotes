using UnityEngine;

namespace CustomNotes.Models;

internal interface INote
{
    GameObject GameObject { get; }
    Transform Transform { get; }
    
    bool UseBaseArrows { get; }
    float ColorMultiplier { get; }
    
    void SetColorScheme(ColorScheme colorScheme);
    void SetBoostColors(ColorScheme colorScheme, bool boost);
    void SetColor(Color color);
}