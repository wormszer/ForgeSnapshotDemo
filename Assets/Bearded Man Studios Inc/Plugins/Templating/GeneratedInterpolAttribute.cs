using System;

namespace BeardedManStudios.Forge.Networking.Generated
{
    [AttributeUsage(AttributeTargets.All)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")] //not needed at runtime do not include in build
    public class GeneratedInterpolAttribute : System.Attribute
    {
        public readonly string JsonData;

        public GeneratedInterpolAttribute(string data)
        {
            this.JsonData = data;
        }

        public override string ToString()
        {
            return string.Format("d:{0}", JsonData);
        }
    }
}
