using CustomNotes.Data;
using CustomNotes.Settings.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssetBundleLoadingTools.Utilities;
using IPA.Utilities;
using UnityEngine;
using Zenject;
using Utils = CustomNotes.Utilities.Utils;

namespace CustomNotes.Managers
{
    public class NoteAssetLoader : IInitializable, IDisposable
    {
        private readonly PluginConfig config;

        private bool isLoaded;

        internal NoteAssetLoader(PluginConfig config)
        {
            this.config = config;
        }

        public List<CustomNote> CustomNoteObjects { get; private set; } = [];
        public List<string> CustomNoteFiles { get; private set; } = [];

        public static string NotesDirectory { get; } = Path.Combine(UnityGame.InstallPath, "CustomNotes");

        public int SelectedNoteIdx { get; set; }
        public bool CustomNoteSelected => SelectedNoteIdx != 0;

        /// <summary>
        /// Load all CustomNotes 
        /// </summary>
        public void Initialize()
        {
            if (isLoaded)
            {
                return;
            }

            Directory.CreateDirectory(NotesDirectory);

            CustomNoteFiles = Utils
                .GetFileNames(NotesDirectory, ["*.bloq", "*.note"], SearchOption.AllDirectories, true)
                .ToList();
            Plugin.Log.Notice($"{CustomNoteFiles.Count} external notes found. Preparing to load.");
            
            CustomNoteObjects = LoadCustomNotes(CustomNoteFiles);
            Plugin.Log.Notice($"{CustomNoteObjects.Count - 1} total custom notes loaded.");

            SelectedNoteIdx = GetSelectedNoteIndex();
            isLoaded = true;
        }

        /// <summary>
        /// Clear all loaded CustomNotes
        /// </summary>
        public void Dispose()
        {
            foreach (var customNote in CustomNoteObjects)
            {
                customNote.Destroy();
            }
            isLoaded = false;
            SelectedNoteIdx = 0;
            CustomNoteObjects.Clear();
            CustomNoteFiles.Clear();
        }

        /// <summary>
        /// Reload all CustomNotes
        /// </summary>
        internal void Reload()
        {
            Plugin.Log.Debug("Reloading the NoteAssetLoader");

            Dispose();
            Initialize();
        }

        private int GetSelectedNoteIndex()
        {
            if (string.IsNullOrWhiteSpace(config.LastNote))
            {
                return 0;
            }
            
            for (int i = 0; i < CustomNoteObjects.Count; i++)
            {
                if (CustomNoteObjects[i].FileName == config.LastNote)
                {
                    return i;
                }
            }
            
            return 0;
        }
        
        public static GameObject LoadNotePrefab(AssetBundle assetBundle, string fileName)
        {
            var noteObject = assetBundle.LoadAsset<GameObject>("assets/_customnote.prefab");
            
            Plugin.Log.Debug($"Repairing shaders for {fileName}");
            var shaderReplacementInfo = ShaderRepair.FixShadersOnGameObject(noteObject);

            if (!shaderReplacementInfo.AllShadersReplaced)
            {
                Plugin.Log.Warn("Missing shader replacement data:");
                foreach (var shaderName in shaderReplacementInfo.MissingShaderNames)
                {
                    Plugin.Log.Warn($"- {shaderName}");
                }
            }
            
            return noteObject;
        } 

        private static List<CustomNote> LoadCustomNotes(IEnumerable<string> customNoteFiles) => 
            customNoteFiles.Prepend("DefaultNotes").Select(CustomNote.Load).ToList();
    }
}