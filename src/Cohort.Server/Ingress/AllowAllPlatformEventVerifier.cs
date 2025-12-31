using Cohort.Adapters.Abstractions;

namespace Cohort.Server.Ingress;

public sealed class AllowAllPlatformEventVerifier : IPlatformEventVerifier
{
    public bool Verify(string platform, string rawBody, IReadOnlyDictionary<string, string> headers) => true;
}

