namespace Cohort.Adapters.Abstractions;

public interface IPlatformEventVerifier
{
    bool Verify(string platform, string rawBody, IReadOnlyDictionary<string, string> headers);
}

