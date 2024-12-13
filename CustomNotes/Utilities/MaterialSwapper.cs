﻿using UnityEngine;
using System.Collections.Generic;

namespace CustomNotes.Utilities;

internal class MaterialSwapper
{
    public static IEnumerable<Material> AllMaterials { get; private set; }

    public static void GetMaterials()
    {
        // This object should be created in the Menu Scene
        // Grab materials from Menu Scene objects
        AllMaterials = Resources.FindObjectsOfTypeAll<Material>();
    }

    public static void ReplaceMaterialsForGameObject(GameObject gameObject)
    {
        AllMaterials ??= Resources.FindObjectsOfTypeAll<Material>();

        foreach (var currentMaterial in AllMaterials)
        {
            string materialName = currentMaterial.name.ToLower() + "_replace (Instance)";
            ReplaceAllMaterialsForGameObjectChildren(gameObject, currentMaterial, materialName);
        }
    }

    public static void ReplaceAllMaterialsForGameObjectChildren(GameObject gameObject, Material material, string materialToReplaceName = "")
    {
        foreach (var renderer in gameObject.GetComponentsInChildren<Renderer>(true))
        {
            ReplaceAllMaterialsForGameObject(renderer.gameObject, material, materialToReplaceName);
        }
    }

    public static void ReplaceAllMaterialsForGameObject(GameObject gameObject, Material material, string materialToReplaceName = "")
    {
        var renderer = gameObject.GetComponent<Renderer>();
        var materialsCopy = renderer.materials;
        bool materialsDidChange = false;

        for (int i = 0; i < renderer.materials.Length; i++)
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