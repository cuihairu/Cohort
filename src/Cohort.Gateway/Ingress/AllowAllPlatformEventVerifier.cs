using Cohort.Adapters.Abstractions;

namespace Cohort.Gateway.Ingress;

public sealed class AllowAllPlatformEventVerifier : IPlatformEventVerifier
{
    public bool Verify(string platform, string rawBody, IReadOnlyDictionary<string, string> headers) => true;
}

