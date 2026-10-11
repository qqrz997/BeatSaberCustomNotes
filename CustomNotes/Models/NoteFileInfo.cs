using System;
using System.IO;

namespace CustomNotes.Models;

public class NoteFileInfo
{
    public NoteFileInfo(FileInfo fileInfo, string hash, DateTime dateAdded)
    {
        FileInfo = fileInfo;
        Hash = hash;
        DateAdded = dateAdded;
    }

    public FileInfo FileInfo { get; }
    public string Hash { get; }
    public DateTime DateAdded { get; }
}
