using UnityEngine;

public static class ColourUtils
{
    public static Color GetColorFromHex(string hexCode)
    {
        if (ColorUtility.TryParseHtmlString(hexCode, out Color color))
            return color;

        Debug.LogWarning($"Invalid hex color: {hexCode}");
        return Color.white; // fallback
    }
}
