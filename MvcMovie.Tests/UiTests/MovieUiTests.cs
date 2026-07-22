using Microsoft.Playwright.Xunit;
using Microsoft.Playwright;
using Xunit;
using System.Threading.Tasks;

namespace MvcMovie.Tests.UiTests
{
    // Inheriting from PageTest provides a clean, isolated browser Page instance per test
    public class MovieUiTests : PageTest
    {
        [Fact] // Standard xUnit attribute
        public async Task Page_MoviesIndex_ShouldRenderCorrectTitle()
        {
            // Act: Direct the headless browser to click and load your local web server port
            // Ensure your target app container is active on port 8080!
            await Page.GotoAsync("http://localhost:8080/Movies");

            // Assert: Extract the visual text from the DOM element using xUnit expectations
            var heading = Page.Locator("h1");
            await Assertions.Expect(heading).ToHaveTextAsync("Index");
        }
    }
}
