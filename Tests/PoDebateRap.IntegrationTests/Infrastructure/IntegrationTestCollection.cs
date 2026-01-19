using Xunit;

namespace PoDebateRap.IntegrationTests.Infrastructure;

/// <summary>
/// Defines a test collection for integration tests that share the same
/// CustomWebApplicationFactory instance and run sequentially.
/// This prevents issues with Serilog's static logger being frozen
/// when multiple tests try to configure it simultaneously.
/// </summary>
[CollectionDefinition("IntegrationTests")]
public class IntegrationTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}

