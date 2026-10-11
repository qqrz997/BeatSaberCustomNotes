using System.Collections.Generic;
using UnityEngine;

namespace CustomNotes.Utilities;

internal class MaterialSwapper
{
    private static Material[] allMaterials;

    public static void ReplaceMaterialsForGameObject(GameObject gameObject)
    {
        allMaterials ??= Resources.FindObjectsOfTypeAll<Material>();

        foreach (var currentMaterial in allMaterials)
        {
            if (currentMaterial == null)
            {
                allMaterials = null;
                ReplaceMaterialsForGameObject(gameObject);
                break;
            }
            
            string materialName = currentMaterial.name.ToLower() + "_replace (Instance)";
            ReplaceAllMaterialsForGameObjectChildren(gameObject, currentMaterial, materialName);
        }
    }

    private static void ReplaceAllMaterialsForGameObjectChildren(GameObject gameObject, Material material, string materialToReplaceName = "")
    {
        foreach (var renderer in gameObject.GetComponentsInChildren<Renderer>(true))
        {
            ReplaceAllMaterialsForGameObject(renderer.gameObject, material, materialToReplaceName);
        }
    }

    private static void ReplaceAllMaterialsForGameObject(GameObject gameObject, Material material, string materialToReplaceName = "")
    {
        var renderer = gameObject.GetComponent<Renderer>();
        var materialsCopy = renderer.materials;
        bool materialsDidChange = false;

        for (int i = 0; i < materialsCopy.Length; i++)
        {
            if (materialsCopy[i].name == materialToReplaceName || materialToReplaceName == "")
            {
                var oldColor = materialsCopy[i].GetColor(MaterialProps.Color);
                materialsCopy[i] = material;
                materialsCopy[i].SetColor(MaterialProps.Color, oldColor);
                materialsDidChange = true;
            }
        }

        if (materialsDidChange)
        {
            renderer.materials = materialsCopy;
        }
    }
}