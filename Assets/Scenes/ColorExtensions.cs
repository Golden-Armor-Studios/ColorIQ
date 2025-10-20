using UnityEngine;

public static class ColorExtensions
{
    public static string ToColorKey(this Color color)
    {
        return ColorUtility.ToHtmlStringRGB(color);
    }
}
