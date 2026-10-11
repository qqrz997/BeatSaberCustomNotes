using System.Collections.Generic;
using CustomNotes.Models;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

namespace CustomNotes.Managers;

internal class GameResourcesManager
{
    private readonly GameObject normalGameNote = LoadResource<GameObject>(
        "Packages/com.beatgames.beatsaber.main.core/Prefabs/SongElements/Notes/NormalGameNote.prefab");
    private readonly GameObject bombNote = LoadResource<GameObject>(
        "Packages/com.beatgames.beatsaber.main.core/Prefabs/SongElements/Notes/BombNote.prefab");

    public DefaultNoteModel CreateNormalGameNote(bool active) => new(normalGameNote, active);
    public DefaultBombModel CreateBombNote(bool active) => new(bombNote, active);

    private static T LoadResource<T>(string label) where T : Object
    {
        var handle = Addressables.LoadAssetAsync<T>(label);
        handle.WaitForCompletion();
        return handle.Status == AsyncOperationStatus.Failed ? throw handle.OperationException : handle.Result;
    }
}