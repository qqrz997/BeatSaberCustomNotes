using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;
using Object = UnityEngine.Object;

namespace CustomNotes;

public class NoteTester : IInitializable
{
    private readonly DiContainer container;

    public NoteTester(DiContainer container)
    {
        this.container = container;
    }

    public void Initialize()
    {
        var keysSet = Addressables.ResourceLocators
            .SelectMany(locator => locator.Keys.Select(key => key.ToString()))
            .Where(key => key.Contains("Packages/com.beatgames.beatsaber.main.core/Prefabs/SongElements"))
            .ToHashSet();
        var sb = new StringBuilder();
        foreach (var key in keysSet) sb.AppendLine(key);
        
        Plugin.Log.Notice(sb.ToString());


        var note = loadprefabandcreateinstance(
            "Packages/com.beatgames.beatsaber.main.core/Prefabs/SongElements/Notes/NormalGameNote.prefab");
        note.transform.SetPositionAndRotation(new(0f, 0.5f, 0f), Quaternion.Euler(90f, 0f, 0f));
        setnotecolor(note, Color.red);

        var chainhead = loadprefabandcreateinstance(
            "Packages/com.beatgames.beatsaber.main.core/Prefabs/SongElements/Notes/BurstSliders/BurstSliderHeadNote.prefab");
        chainhead.transform.SetPositionAndRotation(new(1f, 0.5f, 0), Quaternion.Euler(90f, 0f, 0f));
        setnotecolor(chainhead, Color.red);

        var chainnote = loadprefabandcreateinstance(
            "Packages/com.beatgames.beatsaber.main.core/Prefabs/SongElements/Notes/BurstSliders/BurstSliderNote.prefab");
        chainnote.transform.SetPositionAndRotation(new(2f, 0.5f, 0), Quaternion.Euler(90f, 0f, 0f));
        setnotecolor(chainnote, Color.red);

        var bombnote =
            loadprefabandcreateinstance(
                "Packages/com.beatgames.beatsaber.main.core/Prefabs/SongElements/Notes/BombNote.prefab");
        bombnote.transform.SetPositionAndRotation(new(3f, 0.5f, 0), Quaternion.Euler(90f, 0f, 0f));
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.SetParent(bombnote.transform, false);
        sphere.transform.localScale = new(0.18f, 0.18f, 0.18f);
        
        Mesh mesh = bombnote.transform.Find("Mesh").GetComponent<MeshFilter>().sharedMesh;

        using (var meshDataArray = Mesh.AcquireReadOnlyMeshData(mesh))
        {
            var meshData = meshDataArray[0];

            // Raw vertex buffer
            var vertices = meshData.GetVertexData<Vector3>();

            // Raw index buffer
            var indices = meshData.GetIndexData<ushort>();

            Plugin.Log.Info($"Vertices: {vertices.Length}");
            Plugin.Log.Info($"Indices: {indices.Length}");

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 v = vertices[i];
                Plugin.Log.Info(v.ToString());
            }
        }
        
        logbounds(note);
        logbounds(chainhead);
        logbounds(chainnote);
    }

    static GameObject loadprefabandcreateinstance(string label)
    {
        var handle = Addressables.LoadAssetAsync<GameObject>(label);
        handle.WaitForCompletion();
        var prefab = handle.Result;
        var ins = Object.Instantiate(prefab, Vector3.zero,  Quaternion.identity);
        foreach (var mb in ins.GetComponentsInChildren<MonoBehaviour>()) mb.enabled = false;
        return ins;
    }

    static void setnotecolor(GameObject note, Color color)
    {
        var colorVisuals = note.GetComponentInChildren<ColorNoteVisuals>();
        colorVisuals._noteColor = color;
        foreach (var con in colorVisuals._materialPropertyBlockControllers)
        {
            con.materialPropertyBlock.SetColor(ColorNoteVisuals._colorId, colorVisuals._noteColor with { a = colorVisuals._defaultColorAlpha });
            con.ApplyChanges();
        }
    }

    static void logbounds(GameObject note)
    {
        var noteCube = note.transform.Find("NoteCube");
        var bounds = noteCube.GetComponent<MeshRenderer>().bounds;
        Plugin.Log.Notice($"{note.name}: {bounds.size.x} {bounds.size.y} {bounds.size.z}");
    }
}
