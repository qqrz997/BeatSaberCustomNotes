using System.Collections.Generic;
using CustomNotes.Managers;
using Newtonsoft.Json;

namespace CustomNotes.Models;

internal class Note2Model
{
    [JsonConstructor]
    public Note2Model(
        string iconPath,
        string modelName,
        string authorName,
        Dictionary<AssetPlatform, AssetModel> assets)
    {
        IconPath = iconPath;
        ModelName = modelName;
        AuthorName = authorName;
        Assets = assets;
    }
    
    public string IconPath { get; }
    public string ModelName { get; }
    public string AuthorName { get; }
    public Dictionary<AssetPlatform, AssetModel> Assets { get; }
}