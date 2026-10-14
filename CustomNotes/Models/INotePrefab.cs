namespace CustomNotes.Models;

internal interface INotePrefab
{
    public INote Instantiate();
}