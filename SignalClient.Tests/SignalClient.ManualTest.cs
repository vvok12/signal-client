using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace VVOK12.SignalClient.Tests;

[TestClass]
public class SignalClientManualTest
{
    [TestMethod]
    [DataRow("https://signalbot.one/api/send/", "test message")]
    public  async Task SendMessageAsync(string url, string message)
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient();
        var serviceProvider = services.BuildServiceProvider();

        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var signalClient = new SignalClient(httpClientFactory, url);

        // Act
        var response = await signalClient.SendAsync(message);

        // Assert
        Assert.IsTrue(response.IsSuccessStatusCode, $"Failed to send message. Status code: {response.StatusCode}");
    }
}