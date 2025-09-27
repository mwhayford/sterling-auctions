using NUnit.Framework;

namespace SterlingAuctions.Tests;

/// <summary>
/// Global test configuration and setup for Sterling Auctions tests
/// </summary>
[SetUpFixture]
public class GlobalTestSetup
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // Global test setup
        Console.WriteLine("Starting Sterling Auctions Test Suite");
        
        // Configure test environment
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "InMemory");
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        // Global test cleanup
        Console.WriteLine("Sterling Auctions Test Suite Completed");
    }
}

/// <summary>
/// Base class for all Sterling Auctions tests with common setup
/// </summary>
public abstract class SterlingAuctionsTestBase
{
    protected virtual void SetUp()
    {
        // Common test setup
    }

    protected virtual void TearDown()
    {
        // Common test cleanup
    }

    [SetUp]
    public void TestSetUp()
    {
        SetUp();
    }

    [TearDown]
    public void TestTearDown()
    {
        TearDown();
    }
}
