using System;

[AttributeUsage(AttributeTargets.Assembly)]
public class AssemblyBuildReleaseVersionAttribute : Attribute
{
    public string Version { get; }

    public AssemblyBuildReleaseVersionAttribute(string version)
    {
        Version = version;
    }
}