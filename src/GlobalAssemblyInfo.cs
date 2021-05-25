using System.Reflection;

[assembly: AssemblyProduct("Kryptera")]
[assembly: AssemblyCompany("Gowon, Ltd.")]
[assembly: AssemblyCopyright("Copyright © Gowon, Ltd. 2021")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

#pragma warning disable CS0436 // Type conflicts with imported type
[assembly: AssemblyVersion(ThisAssembly.Version)]
[assembly: AssemblyFileVersion(ThisAssembly.SimpleVersion)]
[assembly: AssemblyInformationalVersion(ThisAssembly.InformationalVersion)]
#pragma warning restore CS0436 // Type conflicts with imported type

// ReSharper disable once CheckNamespace
internal partial class ThisAssembly
{
    /// <summary>
    ///     Simple release-like version number, like 4.0.1 for a cycle 5, SR1 build.
    /// </summary>
    public const string SimpleVersion =
        Git.BaseVersion.Major + "." + Git.BaseVersion.Minor + "." + Git.BaseVersion.Patch;

    /// <summary>
    ///     Full version, including commits since base version file, like 4.0.1.598
    /// </summary>
    public const string Version = SimpleVersion + "." + Git.Commits;

    /// <summary>
    ///     Full version, plus branch and commit short sha, like 4.0.1.598-cycle6+39cf84e
    /// </summary>
    public const string InformationalVersion = Version + "-" + Git.Branch + "+" + Git.Commit;
}