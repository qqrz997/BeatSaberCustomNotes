using CustomNotes.Models;
using UnityEngine;

namespace CustomNotes.Utilities;

internal static class LayerUtils
{
    /// <summary>
    /// Recursively sets the layer of all children of a GameObject.
    /// </summary>
    /// <param name="gameObject">GameObject.</param>
    /// <param name="layer">The layer to recursively set.</param>
    public static void SetLayerRecursively(this GameObject gameObject, NoteLayer layer)
    {
        gameObject.transform.SetLayerRecursively(layer);
    }

    public static void SetLayerRecursively(this Transform transform, NoteLayer layer)
    {
        transform.gameObject.layer = (int)layer;
        for (int index = 0; index < transform.childCount; ++index)
        {
            transform.GetChild(index).SetLayerRecursively(layer);
        }
    }
    
    public static void PrintLayerNames()
    {
        for (int i = 8; i < 32; i++)
        {
            Plugin.Log.Notice($"LayerID:{i} : \"{LayerMask.LayerToName(i)}\"");
        }

        /* | Game Version 1.40.0
        8  : "Note"
        9  : "NoteDebris"
        10 : "Avatar"
        11 : "Obstacle"
        12 : "Saber"
        13 : "NeonLight"
        14 : "Environment"
        15 : "GrabPassTexture1"
        16 : "CutEffectParticles"
        17 : ""
        18 : ""
        19 : "NonReflectedParticles"
        20 : "EnvironmentPhysics"
        21 : ""
        22 : "Event"
        23 : ""
        24 : ""
        25 : "FixMRAlpha"
        26 : ""
        27 : "DontShowInExternalMRCamera"
        28 : "PlayersPlace"
        29 : "Skybox"
        30 : "MRForegroundClipPlane"
        31 : "Reserved"
        */
    }
}