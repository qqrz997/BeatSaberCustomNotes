using System.IO;
using System.Linq;
using UnityEngine;
using static IPA.Loader.PluginManager;
using static SongCore.Collections;

namespace CustomNotes.Utilities;

internal static class Utils
{
    private static Texture2D? defaultIcon;
    private static Texture2D? defaultCustomIcon;
    private static Texture2D? errorIcon;

    public static Texture2D GetDefaultIcon() => 
        defaultIcon ??= LoadEmbeddedImage("CustomNotes.Resources.Icons.default.png");
    public static Texture2D GetDefaultCustomIcon() =>
        defaultCustomIcon ??= LoadEmbeddedImage("CustomNotes.Resources.Icons.defaultCustom.png");
    public static Texture2D GetErrorIcon() =>
        errorIcon ??= LoadEmbeddedImage("CustomNotes.Resources.Icons.error.png");

    private static Texture2D LoadEmbeddedImage(string resourcePath)
    {
        try
        {
            var data = LoadFromResource(resourcePath);
            return LoadTextureRaw(data);
        }
        catch
        {
            Plugin.Log.Error("Failed to load embedded image into a texture.");
            return Texture2D.blackTexture;
        }
    }
    
    private static byte[] LoadFromResource(string resourcePath)
    {
        using var stream = Plugin.ExecutingAssembly.GetManifestResourceStream(resourcePath);
        if (stream == null)
        {
            throw new($"Couldn't find resource at specified path: {resourcePath}");
        }
            
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
            
        return memoryStream.ToArray();
    }

    /// <summary>
    /// Safely unescape \n and \t
    /// </summary>
    /// <param name="text"></param>
    public static string SafeUnescape(string text)
    {
        return string.IsNullOrWhiteSpace(text) ? string.Empty 
            // Unescape just some of the basic formatting characters
            : text.Replace("\\n", "\n").Replace("\\t", "\t");
    }

    /// <summary>
    /// Check if a beatmap requires noodle extensions
    /// </summary>
    public static bool RequiresNoodleExtensions(this BeatmapKey beatmapKey) =>
        EnabledPlugins.Any(x => x.Name == "NoodleExtensions")
        && EnabledPlugins.Any(x => x.Name == "SongCore")
        && beatmapKey.MapHasRequirement("Noodle Extensions");

    private static bool MapHasRequirement(this BeatmapKey beatmapKey, string requirementName) =>
        GetCustomLevelSongDifficultyData(beatmapKey)?
            .additionalDifficultyData
            ._requirements
            .Any(req => req == requirementName) is true;
    
    private static Texture2D LoadTextureRaw(byte[] file)
    {
        if (file.Length > 0)
        {
            var texture = new Texture2D(2, 2);
            if (texture.LoadImage(file))
            {
                return texture;
            }
        }
        return Texture2D.blackTexture;
    }
}