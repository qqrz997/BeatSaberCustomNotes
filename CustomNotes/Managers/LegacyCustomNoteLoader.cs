using System;
using CustomNotes.Models;
using CustomNotes.Utilities;
using UnityEngine;

namespace CustomNotes.Managers;

internal class LegacyCustomNoteLoader
{
    private readonly GameResourcesManager gameResourcesManager;

    public LegacyCustomNoteLoader(GameResourcesManager gameResourcesManager)
    {
        this.gameResourcesManager = gameResourcesManager;
    }

    public CustomNoteData LoadCustomNote(NoteFileInfo file)
    {
        try
        {
            var assetBundle = AssetBundle.LoadFromFile(file.FileInfo.FullName);
            return LoadFromBundle(assetBundle, file);
        }
        catch (Exception ex)
        {
            Plugin.Log.Warn($"Problem encountered when loading '{file.FileInfo.Name}'\n{ex}");

            var message = $"File: '{file.FileInfo.Name}'" +
                          "\n\nThis file failed to load." +
                          "\n\nThis may have been caused by having duplicated files, another note with the" +
                          " same name already exists or that the custom note is simply just broken." +
                          "\n\nThe best thing is probably just to delete it!";
            return CustomNoteData.Error(file, message);
        }
    }

    private CustomNoteData LoadFromBundle(AssetBundle assetBundle, NoteFileInfo file)
    {
        var prefab = assetBundle.LoadAsset<GameObject>("assets/_customnote.prefab");

        var descriptor = prefab.GetComponent<NoteDescriptor>();
        descriptor.Icon ??= Utils.GetDefaultCustomIcon();

        var noteArrowLeft = new LegacyNoteSubPrefab(prefab.transform.Find("NoteLeft").gameObject);
        var noteDotLeft = GetSubPrefab("NoteDotLeft", noteArrowLeft);
        var chainArrowLeft = GetOrCreateSubPrefab("BurstSliderHeadLeft", new(0f, 0.255f, 0f), new(1f, 0.59f, 1f), noteArrowLeft);
        var chainDotLeft = GetSubPrefab("BurstSliderHeadDotLeft", chainArrowLeft);
        var chainSegmentLeft = GetOrCreateSubPrefab("BurstSliderLeft", Vector3.zero, new(1f, 0.225f, 1f), noteDotLeft);
        
        var noteArrowRight = new LegacyNoteSubPrefab(prefab.transform.Find("NoteRight").gameObject);
        var noteDotRight = GetSubPrefab("NoteDotRight", noteArrowRight);
        var chainArrowRight = GetOrCreateSubPrefab("BurstSliderHeadRight", new(0f, 0.255f, 0f), new(1f, 0.59f, 1f), noteArrowRight);
        var chainDotRight = GetSubPrefab("BurstSliderHeadDotRight", chainArrowRight);
        var chainSegmentRight = GetOrCreateSubPrefab("BurstSliderRight", Vector3.zero, new(1f, 0.225f, 1f), noteDotRight);
        
        var bomb = GetBomb("NoteBomb");
        
        var noteInfo = new NoteInfo(descriptor.NoteName, descriptor.AuthorName, descriptor.Icon);
        var metadata = new NoteMetadata(file, noteInfo, string.Empty);
        
        var customNotePrefab = new LegacyNotePrefab(
            noteArrowLeft, noteDotLeft, chainArrowLeft, chainDotLeft, chainSegmentLeft,
            noteArrowRight, noteDotRight,  chainArrowRight, chainDotRight, chainSegmentRight,
            bomb,
            descriptor.UsesNoteColor, descriptor.NoteColorStrength);
            
        return new(assetBundle, metadata, customNotePrefab);

        LegacyNoteSubPrefab GetSubPrefab(string subPrefabName, LegacyNoteSubPrefab fallback)
        {
            var subPrefab = prefab.transform.Find(subPrefabName);
            return !subPrefab ? fallback : new(subPrefab.gameObject);
        }
        
        LegacyNoteSubPrefab GetOrCreateSubPrefab(string subPrefabName,
            Vector3 position,
            Vector3 scale,
            LegacyNoteSubPrefab fallback)
        {
            var subPrefab = prefab.transform.Find(subPrefabName)?.gameObject;
            if (subPrefab == null)
            {
                subPrefab = new(subPrefabName);
                subPrefab.transform.SetParent(prefab.transform);

                var model = fallback.Instantiate().GameObject;
                model.transform.SetParent(subPrefab.transform);
                model.transform.localPosition = position;
                model.transform.localScale = scale;
            }

            return new(subPrefab, !descriptor.DisableBaseNoteArrows, descriptor.NoteColorStrength);
        }

        LegacyNoteSubPrefab GetBomb(string bombName)
        {
            var noteBomb = prefab.transform.Find(bombName)?.gameObject;
            if (noteBomb == null)
            {
                noteBomb = new(bombName);
                noteBomb.transform.SetParent(prefab.transform);

                var model = gameResourcesManager.CreateBombNote(true);
                model.Transform.SetParent(noteBomb.transform, true);
                model.Transform.localPosition = Vector3.zero;
                model.Transform.localScale = Vector3.one / 0.4f;
            }

            return new(noteBomb, !descriptor.DisableBaseNoteArrows, descriptor.NoteColorStrength);
        }
    }
}