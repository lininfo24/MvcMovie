using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Moq; // A standard mocking library if you need to mock dependencies later

using MvcMovie.Controllers;

using Xunit;

namespace MvcMovie.Tests;

public class HomeControllerTests
{
    [Fact]
    public void Index_ReturnsAViewResult_WithALogger()
    {
        // 1. Arrange: Set up the dependencies your Controller needs
        // We create a mock logger so the controller can run without hitting real disk systems
        var mockLogger = new Mock<ILogger<HomeController>>();
        var controller = new HomeController(mockLogger.Object);

        // 2. Act: Call the target method on your MVC controller
        var result = controller.Index();

        // 3. Assert: Verify the output matches engineering expectations
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.ViewName); // Standard ASP.NET behavior when returning default view
    }
}
