using UnityEngine;

namespace CustomNotes.Models;

internal class NoteInfo
{
    public NoteInfo(string name, string author, Texture2D icon)
    {
        Name = name;
        Author = author;
        Icon = icon;
    }

    public string Name { get; }
    public string Author { get; }
    public Texture2D Icon { get; }
}