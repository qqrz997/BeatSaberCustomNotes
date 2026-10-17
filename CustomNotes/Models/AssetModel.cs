using Newtonsoft.Json;

namespace CustomNotes.Models;

internal class AssetModel
{
    [JsonConstructor]
    public AssetModel(
        string filePath)
    {
        FilePath = filePath;
    }
    
    public string FilePath { get; }
}