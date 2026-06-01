using UnityEngine;

namespace FrostShelter.Utils
{
    public static class MathHelpers
    {
        /// <summary>线性插值（未限制clamp）</summary>
        public static float LerpUnclamped(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        /// <summary>将值从一个范围映射到另一个范围</summary>
        public static float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            float t = Mathf.InverseLerp(fromMin, fromMax, value);
            return Mathf.Lerp(toMin, toMax, t);
        }

        /// <summary>加权随机选择索引</summary>
        public static int WeightedRandomIndex(float[] weights, float totalWeight = -1f)
        {
            if (weights == null || weights.Length == 0) return 0;

            if (totalWeight < 0f)
            {
                totalWeight = 0f;
                foreach (var w in weights) totalWeight += w;
            }

            float roll = Random.value * totalWeight;
            float cumulative = 0f;

            for (int i = 0; i < weights.Length; i++)
            {
                cumulative += weights[i];
                if (roll <= cumulative) return i;
            }

            return weights.Length - 1;
        }

        /// <summary>确保值在范围内</summary>
        public static float Clamp(float value, float min, float max) => Mathf.Clamp(value, min, max);
        public static int Clamp(int value, int min, int max) => Mathf.Clamp(value, min, max);
    }
}
