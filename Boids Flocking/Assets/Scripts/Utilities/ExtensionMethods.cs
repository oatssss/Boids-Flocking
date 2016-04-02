using UnityEngine;

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
    }
}