using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;

using Xunit;

namespace MvcMovie.Tests;

// Program is your main web app's entry point
public class MovieIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public MovieIntegrationTests(WebApplicationFactory<Program> factory)
    {
        // This boots your entire app (and your SQLite In-Memory DB) in RAM
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_MoviesIndex_ReturnsSuccessAndSeedData()
    {
        // Act: Simulate a user browsing to the Movies page
        var response = await _client.GetAsync("/Movies");

        // Assert 1: Ensure the page returns an HTTP 200 OK status
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Assert 2: Read the HTML and verify your SeedData is present on the page
        var htmlResult = await response.Content.ReadAsStringAsync();
        Assert.Contains("Ghostbusters", htmlResult);
    }
}
