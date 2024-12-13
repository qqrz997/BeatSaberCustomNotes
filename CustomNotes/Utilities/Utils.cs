using System.IO;
using System.Linq;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using CustomNotes.Components.CustomNotes;
using IPA.Loader;
using SongCore;

namespace CustomNotes.Utilities;

internal static class Utils
{
    /// <summary>
    /// Colorize a Note based on a ColorManager and CustomNote configuration
    /// </summary>
    /// <param name="color">Color</param>
    /// <param name="colorStrength">Color strength</param>
    /// <param name="noteObject">Note to colorize</param>
    public static void ColorizeCustomNote(Color color, float colorStrength, GameObject noteObject)
    {
        if (noteObject == null)
        {
            return;
        }

        var noteColor = color * colorStrength;
        var childTransforms = noteObject.GetComponentsInChildren<Transform>();
            
        foreach (var transform in childTransforms)
        {
            if (transform.GetComponent<DisableNoteColorOnGameobject>() != null)
            {
                continue;
            }

            if (transform.TryGetComponent<Renderer>(out var renderer))
            {
                renderer.material.SetColor(MaterialProps.Color, noteColor);
            }
        }
            
        if (noteObject.TryGetComponent<MaterialPropertyBlockController>(out var materialPropertyBlockController))
        {
            // Set the color of material property block controllers, for the replaced note shader
            materialPropertyBlockController.materialPropertyBlock.SetColor(MaterialProps.Color, noteColor);
            materialPropertyBlockController.ApplyChanges();
        }
    }

    /// <summary>
    /// Adds a MaterialPropertyBlockController to the root of the gameObject. Only selects renderers with specific shaders.
    /// </summary>
    /// <param name="gameObject"></param>
    public static void AddMaterialPropertyBlockController(GameObject gameObject)
    {
        var rendererList = gameObject.GetComponentsInChildren<Renderer>()
            .Where(renderer => 
                renderer.GetComponent<DisableNoteColorOnGameobject>() == null
                && renderer.material.shader.name == "Custom/NoteHD")
            .ToArray();

        if (rendererList.Length > 0)
        {
            var newController = gameObject.AddComponent<MaterialPropertyBlockController>();
            newController._renderers = rendererList;
        }
    }

    public static byte[] LoadFromResource(string resourcePath)
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

    private static Texture2D defaultIcon;
    private static Texture2D defaultCustomIcon;
    private static Texture2D errorIcon;

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
            return LoadTextureRaw(LoadFromResource(resourcePath));
        }
        catch
        {
            Plugin.Log.Error("Failed to load embedded image into a texture.");
            return null;
        }
    }

    /// <summary>
    /// Loads an Texture2D from byte[]
    /// </summary>
    /// <param name="file"></param>
    public static Texture2D LoadTextureRaw(byte[] file)
    {
        if (file.Length > 0)
        {
            var texture = new Texture2D(2, 2);
            if (texture.LoadImage(file))
            {
                return texture;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets every file matching the filter in a path.
    /// </summary>
    /// <param name="path">Directory to search in.</param>
    /// <param name="filters">Pattern(s) to search for.</param>
    /// <param name="searchOption">Search options.</param>
    /// <param name="returnShortPath">Remove path from filepaths.</param>
    public static IEnumerable<string> GetFileNames(string path, IEnumerable<string> filters, SearchOption searchOption, bool returnShortPath = false)
    {
        IList<string> filePaths = new List<string>();

        foreach (string filter in filters)
        {
            IEnumerable<string> directoryFiles = Directory.GetFiles(path, filter, searchOption);

            if (returnShortPath)
            {
                foreach (string directoryFile in directoryFiles)
                {
                    string filePath = directoryFile.Replace(path, "");
                    if (filePath.Length > 0 && filePath.StartsWith(@"\"))
                    {
                        filePath = filePath.Substring(1, filePath.Length - 1);
                    }

                    if (!string.IsNullOrWhiteSpace(filePath) && !filePaths.Contains(filePath))
                    {
                        filePaths.Add(filePath);
                    }
                }
            }
            else
            {
                filePaths = filePaths.Union(directoryFiles).ToList();
            }
        }

        return filePaths.Distinct();
    }

    /// <summary>
    /// Safely unescape \n and \t
    /// </summary>
    /// <param name="text"></param>
    public static string SafeUnescape(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        // Unescape just some of the basic formatting characters
        return text.Replace("\\n", "\n").Replace("\\t", "\t");
    }

    /// <summary>
    /// Check if a beatmap requires noodle extensions
    /// </summary>
    public static bool RequiresNoodleExtensions(this GameplayCoreSceneSetupData setupData) =>
        PluginManager.EnabledPlugins.Any(x => x.Name == "NoodleExtensions")
        && PluginManager.EnabledPlugins.Any(x => x.Name == "SongCore")
        && setupData.MapHasRequirement("Noodle Extensions");

    private static bool MapHasRequirement(this GameplayCoreSceneSetupData setupData, string requirementName) =>
        Collections.RetrieveDifficultyData(setupData.beatmapLevel, setupData.beatmapKey)?
            .additionalDifficultyData
            ._requirements
            .Any(req => req == requirementName) is true;
}