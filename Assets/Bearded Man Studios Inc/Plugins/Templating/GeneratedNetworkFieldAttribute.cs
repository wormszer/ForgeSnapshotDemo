using System;

namespace BeardedManStudios.Forge.Networking.Generated
{
    [AttributeUsage(AttributeTargets.Field)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")] //not needed at runtime do not include in build
    public class GeneratedNetworkFieldAttribute : System.Attribute
    {
        //public readonly string JsonData;
        public float InterpolationValue = 0;

        public GeneratedNetworkFieldAttribute(float interpolation_value)
        {
            InterpolationValue = interpolation_value;
        }

        public GeneratedNetworkFieldAttribute()
        {
            InterpolationValue = 0;
        }

        public override string ToString()
        {
            return string.Format("d:{0}", InterpolationValue);
        }
    }
}
