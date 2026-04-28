using System.Collections.Generic;
using Assets._Scripts.Enums;
using UnityEngine;

namespace Assets._Scripts.Helpers
{
    public static class ColorMapper
    {
        private static Dictionary<EColor, Color> _colorDict = new()
        {
            {EColor.Blue, new Color(.33f, .46f, .79f)},
            {EColor.Green, new Color(.39f, .76f, .34f)},
            {EColor.Red, new Color(.71f, .21f, .29f)},
            {EColor.Yellow, new Color(.77f, .77f, .22f)},
            {EColor.Pink, new Color(.8f, .33f, .7f)},
            {EColor.Cyan, new Color(.3f, .7f, .67f)},
            {EColor.Purple, new Color(.46f, .44f, .8f)},
            {EColor.Brown, new Color(.33f, .46f, .79f)},
            {EColor.Ocean, new Color(.33f, .62f, .8f)},
        };

        public static IEnumerable<EColor> GetAllColors() => _colorDict.Keys;

        public static Color GetColor(EColor key) => _colorDict.TryGetValue(key, out var color) ? color : Color.clear;
    }
}