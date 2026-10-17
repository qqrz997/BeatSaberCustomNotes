using UnityEngine;

namespace CustomNotes.Models;

internal interface INoteModel
{
    GameObject GameObject { get; }
    Transform Transform { get; }
    
    void SetColor(Color color);
}