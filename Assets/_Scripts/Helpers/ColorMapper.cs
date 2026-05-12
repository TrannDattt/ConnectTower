using System.Collections.Generic;
using Assets._Scripts.Enums;
using UnityEngine;

namespace Assets._Scripts.Helpers
{
    public static class ColorMapper
    {
        private static Dictionary<EColor, Color> _colorDict = new()
        {
            {EColor.DarkBlue, new Color(.33f, .46f, .79f)},
            {EColor.Green, new Color(.29f, 1f, .21f)},
            {EColor.Red, new Color(.71f, .21f, .29f)},
            {EColor.Yellow, new Color(.77f, .77f, .22f)},
            {EColor.Pink, new Color(.8f, .33f, .7f)},
            {EColor.Cyan, new Color(0f, 1f, .68f)},
            {EColor.Purple, new Color(.46f, .44f, .8f)},
            {EColor.Brown, new Color(.33f, .46f, .79f)},
            {EColor.Orange, new Color(1f, .5f, 0f)},
            {EColor.LightBlack, new Color(.36f, .36f, .36f)},
        };

        public static IEnumerable<EColor> GetAllColors() => _colorDict.Keys;

        public static Color GetColor(EColor key) => _colorDict.TryGetValue(key, out var color) ? color : Color.clear;
    }
}