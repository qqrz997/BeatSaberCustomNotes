using System;
using CustomNotes.Models;
using CustomNotes.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

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

        var noteArrowLeft = prefab.transform.Find("NoteLeft").gameObject;
        var noteArrowRight = prefab.transform.Find("NoteRight").gameObject;
        var noteDotLeftTransform = prefab.transform.Find("NoteDotLeft");
        var noteDotRightTransform = prefab.transform.Find("NoteDotRight");
        var noteDotLeft = noteDotLeftTransform != null ? noteDotLeftTransform.gameObject : noteArrowLeft;
        var noteDotRight = noteDotRightTransform != null ? noteDotRightTransform.gameObject : noteArrowRight;
       
        var noteBomb = GetBomb("NoteBomb");
        
        var chainSegmentLeft = GetSubPrefab(noteDotLeft, "BurstSliderLeft", Vector3.zero, new(1f, 0.225f, 1f));
        var chainSegmentRight = GetSubPrefab(noteDotLeft, "BurstSliderRight", Vector3.zero, new(1f, 0.225f, 1f));

        var chainArrowLeft = GetSubPrefab(noteArrowLeft, "BurstSliderHeadLeft", new(0f, 0.255f, 0f), new(1f, 0.59f, 1f));
        var chainArrowRight = GetSubPrefab(noteArrowRight, "BurstSliderHeadRight", new(0f, 0.255f, 0f), new(1f, 0.59f, 1f));
        
        var chainDotLeft = GetSubPrefab(noteDotLeft, "BurstSliderHeadDotLeft", new(0f, 0.255f, 0f), new(1f, 0.59f, 1f));
        var chainDotRight = GetSubPrefab(noteDotRight, "BurstSliderHeadDotRight", new(0f, 0.255f, 0f), new(1f, 0.59f, 1f));

        var noteInfo = new NoteInfo(descriptor.NoteName, descriptor.AuthorName, descriptor.Icon);
        var metadata = new NoteMetadata(file, noteInfo, string.Empty);
        
        var leftPrefabs = NotePrefabs.LegacyNote(
            noteArrowLeft, noteDotLeft, chainArrowLeft, chainDotLeft, chainSegmentLeft, 
            !descriptor.DisableBaseNoteArrows, descriptor.NoteColorStrength);
        
        var rightPrefabs = NotePrefabs.LegacyNote(
            noteArrowRight, noteDotRight, chainArrowRight, chainDotRight, chainSegmentRight, 
            !descriptor.DisableBaseNoteArrows, descriptor.NoteColorStrength);
        
        var bombPrefab = new LegacyNotePrefab(noteBomb, !descriptor.DisableBaseNoteArrows, descriptor.NoteColorStrength);
        
        var customNotePrefab = new CustomNotePrefab(
            leftPrefabs, rightPrefabs, bombPrefab, descriptor.UsesNoteColor, descriptor.NoteColorStrength);
        
        return new(assetBundle, metadata, customNotePrefab);

        GameObject GetSubPrefab(GameObject fallback, string subPrefabName, Vector3 position, Vector3 scale)
        {
            var subPrefab = prefab.transform.Find(subPrefabName)?.gameObject;
            if (subPrefab != null) return subPrefab;

            subPrefab = new(subPrefabName);
            subPrefab.transform.SetParent(prefab.transform);
            
            var model = Object.Instantiate(fallback, subPrefab.transform, true);
            model.transform.localPosition = position;
            model.transform.localScale = scale;
        
            return subPrefab;
        }

        GameObject GetBomb(string bombName)
        {
            var bomb = prefab.transform.Find(bombName)?.gameObject;
            if (bomb != null) return bomb;

            bomb = new(bombName);
            bomb.transform.SetParent(prefab.transform);

            var model = gameResourcesManager.CreateBombNote(true);
            model.Transform.SetParent(bomb.transform, true);
            model.Transform.localPosition = Vector3.zero;
            model.Transform.localScale = Vector3.one / 0.4f;
        
            return bomb;
        }
    }
}