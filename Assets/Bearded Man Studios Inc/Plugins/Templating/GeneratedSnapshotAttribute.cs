using System;

namespace BeardedManStudios.Forge.Networking.Generated
{
    [AttributeUsage(AttributeTargets.Class)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")] //not needed at runtime do not include in build
    public class GeneratedSnapshotAttribute : System.Attribute
    {
        public GeneratedSnapshotAttribute()
        {
        }

        public override string ToString()
        {
            return string.Format("snap");
        }
    }
}
