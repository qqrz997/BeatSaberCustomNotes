using UnityEngine;
using Zenject;

namespace CustomNotes.Models;

/// <summary>
/// Poolable container for creating multiple note instances
/// </summary>
internal class NoteInstanceContainer : MonoBehaviour
{
    [SerializeField]
    private MonoBehaviour instance = null!;

    // Not serializable by unity. Storing it as MonoBehaviour to get around this 
    public INote Instance => (INote)instance;

    public static NoteInstanceContainer Create(INote note)
    {
        if (note is not MonoBehaviour instance)
        {
            throw new($"Provided {nameof(INote)} must be a {nameof(MonoBehaviour)} to be serialized by Unity");
        }
        
        var container = new GameObject(nameof(NoteInstanceContainer)).AddComponent<NoteInstanceContainer>();   
        container.instance = instance;
        note.Transform.SetParent(container.transform);
        note.Transform.localPosition = Vector3.zero;
        container.gameObject.SetActive(false);
        return container;
    }
    
    public class Pool : MonoMemoryPool<NoteInstanceContainer>;
}