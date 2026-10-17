namespace CustomNotes.Models;

internal class NoteMetadata
{
    public NoteMetadata(NoteFileInfo file, NoteInfo info, string errorMessage)
    {
        File = file;
        Info = info;
        ErrorMessage = errorMessage;
    }
    
    public NoteFileInfo File { get; }
    public NoteInfo Info { get; } 
    public string ErrorMessage { get; }
}