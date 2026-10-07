using UnityEngine;
using Zenject;

namespace CustomNotes.Installers;

internal class CustomNoteInstance : MonoBehaviour
{
    public GameObject instance;
    public NoteColorizer colorizer;

    public void Init(GameObject prefab)
    {
        instance = Instantiate(prefab, transform, true);
        instance.SetActive(false);
        colorizer = instance.AddComponent<NoteColorizer>();
    }
    
    public class Pool : MonoMemoryPool<CustomNoteInstance>;
}