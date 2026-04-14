using System.Collections.Generic;
using Assets._Scripts.Enums;
using UnityEngine;

namespace Assets._Scripts.Helpers
{
    public static class ColorMapper
    {
        private static Dictionary<EColor, Color> _colorDict = new()
        {
            {EColor.Blue, new(.33f, .46f, .79f)},
            {EColor.Green, new(.39f, .76f, .34f)},
            {EColor.Red, new(.71f, .21f, .29f)},
            {EColor.Yellow, new(.77f, .77f, .22f)},
            {EColor.Pink, new(.8f, .33f, .7f)},
            {EColor.Cyan, new(.3f, .7f, .67f)},
            {EColor.Purple, new(.46f, .44f, .8f)},
            {EColor.Brown, new(.33f, .46f, .79f)},
            {EColor.Ocean, new(.33f, .62f, .8f)},
            {EColor.Gray, new(.78f, .78f, .78f)},
        };

        public static IEnumerable<EColor> GetAllColors() => _colorDict.Keys;

        public static Color GetColor(EColor key) => _colorDict.TryGetValue(key, out var color) ? color : Color.clear;
    }
}