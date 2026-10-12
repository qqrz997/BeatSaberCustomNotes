using UnityEngine;
using Zenject;

namespace CustomNotes.Models;

/// <summary>
/// Poolable container for creating multiple note instances
/// </summary>
internal class NoteInstanceContainer : MonoBehaviour
{
    public NoteInstance instance = null!;

    public static NoteInstanceContainer Create(NoteInstance instance)
    {
        var container = new GameObject(nameof(NoteInstanceContainer)).AddComponent<NoteInstanceContainer>();    
        container.instance = instance;
        instance.transform.SetParent(container.transform);
        instance.transform.localPosition = Vector3.zero;
        container.gameObject.SetActive(false);
        return container;
    }
    
    public class Pool : MonoMemoryPool<NoteInstanceContainer>;
}