using System;

namespace BeardedManStudios.Forge.Networking.Generated
{
    [AttributeUsage(AttributeTargets.All)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")] //not needed at runtime do not include in build
    public class GeneratedRPCAttribute : System.Attribute
    {
        public readonly string JsonData;
        public readonly string[] Names;
        public readonly string[] Types;

        public GeneratedRPCAttribute(string data)
        {
            this.JsonData = data;
        }

        public GeneratedRPCAttribute(string[] names, string[] types)
        {
            this.Names = names;
            this.Types = types;
        }

        public override string ToString()
        {
            return string.Format("d:{0}", JsonData);
        }
    }
}
