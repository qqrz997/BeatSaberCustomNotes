using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomNotes.Utilities;

internal static class MaterialSwapper
{
    private static Dictionary<string, Material> replacements;

    public static void ReplaceSwappableMaterials(this GameObject gameObject)
    {
        replacements ??= GetReplacementCandidates();

        foreach (var renderer in gameObject.GetComponentsInChildren<Renderer>(true))
        {
            var materials = renderer.sharedMaterials;
            var changed = false;

            for (var i = 0; i < materials.Length; i++)
            {
                var oldMaterial = materials[i];
                if (!oldMaterial || !replacements.TryGetValue(oldMaterial.name, out var replacement))
                {
                    continue;
                }

                materials[i] = new(replacement)
                {
                    color = oldMaterial.color
                };
                
                changed = true;
            }

            if (changed)
            {
                renderer.materials = materials;
            }
        }
    }

    private static Dictionary<string, Material> GetReplacementCandidates()
    {
        var replacements = new Dictionary<string, Material>(StringComparer.OrdinalIgnoreCase);

        foreach (var material in Resources.FindObjectsOfTypeAll<Material>())
        {
            if (material) replacements[material.name + "_replace"] = material;
        }

        return replacements;
    }
}