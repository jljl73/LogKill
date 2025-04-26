using LogKill.Character;
using System.Collections.Generic;
using UnityEngine;

namespace LogKill.Utils
{
    public class Util
    {
        public static void Suffle<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        public static Color GetColor(EColorType colorType)
        {
            switch (colorType)
            {
                case EColorType.White:
                    return Color.white;
                case EColorType.Red:
                    return Color.red;
                case EColorType.Blue:
                    return Color.blue;
                case EColorType.Green:
                    return Color.green;
                case EColorType.Yellow:
                    return Color.yellow;
                case EColorType.Purple:
                    return new Color(0.5f, 0f, 0.5f);
                case EColorType.Gray:
                    return Color.gray;
                case EColorType.Pink:
                    return new Color(1f, 0.75f, 0.8f);
                case EColorType.Orange:
                    return new Color(1f, 0.65f, 0f);
                case EColorType.Cyan:
                    return Color.cyan;
                default:
                    return Color.white;
            }
        }
    }
}