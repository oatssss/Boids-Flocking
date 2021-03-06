﻿using UnityEngine;
using System.Collections.Generic;

namespace ExtensionMethods
{
    public static class ExtensionMethods
    {
        public static Vector3 NormalizeMagnitudeToRange(this Vector3 value, float fromRangeMin, float fromRangeMax, float toRangeMin, float toRangeMax)
        {
            float magnitude = value.magnitude;

            if (magnitude <= 0f)
                { return Vector3.zero; }

            float magInStandardRange = Mathf.InverseLerp(fromRangeMin, fromRangeMax, magnitude);
            float magInGivenRange = Mathf.Lerp(toRangeMin, toRangeMax, magInStandardRange);

            float scalar = magInGivenRange/magnitude;
            return value * scalar;
        }

        public static Vector3 NormalizeMagnitudeToRange(this Vector3 value, float fromRangeMax, float toRangeMin, float toRangeMax)
        {
            return value.NormalizeMagnitudeToRange(0f, fromRangeMax, toRangeMin, toRangeMax);
        }

        public static Vector3 ClampMagnitudeToRange(this Vector3 value, float min, float max)
        {
            float mag = value.magnitude;
            Vector3 clamped = value;

            // Magnitudes can't be negative
            min = min < 0 ? 0 : min;

            if (mag < min)
                { clamped = value.normalized*min; }

            else if (mag > max)
                { clamped = value.normalized*max; }

            return clamped;
        }

        public static void Populate<V>(this V[] arr, V value )
        {
            for (int i = 0; i < arr.Length; i++)
                { arr[i] = value; }
        }

        public static void EnforceLayerMembership(this MonoBehaviour behaviour, string layer)
        {
            int layerIndex = LayerMask.NameToLayer(layer);

            if (behaviour.gameObject.layer != layerIndex)
            {
                Debug.LogWarningFormat("{0} does not belong to {1}, reassigning...", behaviour, layer);
                behaviour.gameObject.layer = layerIndex;
            }
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1) {
                n--;
                int k = Random.Range(0,n);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}