using System.Runtime.CompilerServices;
using VerifyTests;
using VerifyTests.DiffPlex;
using VerifyXunit;

namespace Pathy.ApiVerificationTests;

public static class ModuleInit
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifyDiffPlex.Initialize(OutputType.Minimal);
        Verifier.UseProjectRelativeDirectory("ApprovedApi");
        VerifierSettings.ScrubLinesContaining("FrameworkDisplayName");
    }
}
