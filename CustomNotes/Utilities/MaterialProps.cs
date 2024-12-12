using UnityEngine;

namespace CustomNotes.Utilities;

internal static class MaterialProps
{
    public static int Color { get; } = Shader.PropertyToID("_Color");
}