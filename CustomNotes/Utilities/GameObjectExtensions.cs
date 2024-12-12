using UnityEngine;

namespace CustomNotes.Utilities;

internal static class GameObjectExtensions
{
    public static bool TryGetComponent<T>(this GameObject gameObject, out T component) where T : Component
    {
        component = gameObject.GetComponent<T>();
        return component != null;
    }
    
    public static bool TryGetComponent<T>(this Transform transform, out T component) where T : Component
    {
        component = transform.GetComponent<T>();
        return component != null;
    }
    
    public static T TryGetComponentOrAdd<T>(this GameObject obj) where T : Component =>
        obj.GetComponent<T>() ?? obj.AddComponent<T>();
}