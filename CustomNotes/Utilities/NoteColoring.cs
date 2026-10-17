using System;
using AssetComponents.Models;
using UnityEngine;

namespace CustomNotes.Utilities;

internal static class NoteColoring
{
    public static bool UsesSaberColors(this IBeatSaberColorer colorer) => colorer.ColorSchemeType 
        is ColorSchemeType.LeftSaber or ColorSchemeType.RightSaber;

    public static bool UsesBoostColors(this IBeatSaberColorer colorer) => colorer is {
        UseColorBoostEvents: true,
        ColorSchemeType: ColorSchemeType.EnvironmentColor0 or ColorSchemeType.EnvironmentColor1
        or ColorSchemeType.EnvironmentColorW or ColorSchemeType.EnvironmentColor0Boost
        or ColorSchemeType.EnvironmentColor1Boost or ColorSchemeType.EnvironmentColorWBoost
    };

    public static Color GetColorByType(this ColorScheme s, ColorSchemeType t) => t switch
    {
        ColorSchemeType.LeftSaber => s.saberAColor,
        ColorSchemeType.RightSaber => s.saberBColor,
        ColorSchemeType.EnvironmentColor0 => s.environmentColor0,
        ColorSchemeType.EnvironmentColor1 => s.environmentColor1,
        ColorSchemeType.EnvironmentColorW => s.environmentColorW,
        ColorSchemeType.EnvironmentColor0Boost => s.environmentColor0Boost,
        ColorSchemeType.EnvironmentColor1Boost => s.environmentColor1Boost,
        ColorSchemeType.EnvironmentColorWBoost => s.environmentColorWBoost,
        ColorSchemeType.ObstaclesColor => s.obstaclesColor,
        _ => throw new ArgumentOutOfRangeException(nameof(t), t, null)
    };

    public static Color GetBoostColorByType(this ColorScheme s, ColorSchemeType t, bool boost) => (t, boost) switch
    {
        (ColorSchemeType.LeftSaber, _) => s.saberAColor,
        (ColorSchemeType.RightSaber, _) => s.saberBColor,
        (ColorSchemeType.ObstaclesColor, _) => s.obstaclesColor,
        (ColorSchemeType.EnvironmentColor0, false) => s.environmentColor0,
        (ColorSchemeType.EnvironmentColor1, false) => s.environmentColor1,
        (ColorSchemeType.EnvironmentColorW, false) => s.environmentColorW,
        (ColorSchemeType.EnvironmentColor0Boost, false) => s.environmentColor0Boost,
        (ColorSchemeType.EnvironmentColor1Boost, false) => s.environmentColor1Boost,
        (ColorSchemeType.EnvironmentColorWBoost, false) => s.environmentColorWBoost,
        (ColorSchemeType.EnvironmentColor0, true) => s.environmentColor0Boost,
        (ColorSchemeType.EnvironmentColor1, true) => s.environmentColor1Boost,
        (ColorSchemeType.EnvironmentColorW, true) => s.environmentColorWBoost,
        (ColorSchemeType.EnvironmentColor0Boost, true) => s.environmentColor0,
        (ColorSchemeType.EnvironmentColor1Boost, true) => s.environmentColor1,
        (ColorSchemeType.EnvironmentColorWBoost, true) => s.environmentColorW,
        _ => throw new ArgumentOutOfRangeException()
    };
}