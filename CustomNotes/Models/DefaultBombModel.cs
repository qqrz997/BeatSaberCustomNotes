using UnityEngine;

namespace CustomNotes.Models;

internal class DefaultBombModel : INoteModel
{
    public DefaultBombModel(GameObject bombPrefab, bool active)
    {
        var instance = Object.Instantiate(bombPrefab);
        
        var mesh = instance.transform.Find("Mesh");
        mesh.SetParent(null);
        mesh.localPosition = Vector3.zero;

        Object.Destroy(instance);
        
        foreach (var monoBehaviour in mesh.GetComponentsInChildren<MonoBehaviour>())
            monoBehaviour.enabled = false;
        
        GameObject = mesh.gameObject;
        GameObject.SetActive(active);
        Transform = mesh;
    }
    
    public GameObject GameObject { get; }
    public Transform Transform { get; }
    
    public void SetColor(Color color) { }
}