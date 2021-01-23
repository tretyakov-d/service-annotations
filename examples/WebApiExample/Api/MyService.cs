using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceAnnotations;

namespace WebApiExample.Api
{
    [Service(ServiceLifetime.Singleton), ConfigureServices(nameof(RegisterRandom))]
    public class MyService
    {
        readonly HttpClient _httpClient;

        public MyService(HttpClient httpClient) => _httpClient = httpClient;

        public MyResponse CreateResponse() => new()
        {
            Message = "Response created by my service, " +
                      "which was registered using ServiceAttribute and also configured it's own dependent HttpClient, " +
                      "check the base address to be sure.",
            BaseAddress = _httpClient.BaseAddress
        };

        static void RegisterRandom(IServiceCollection serviceCollection, IConfiguration configuration)
            => serviceCollection.AddHttpClient<MyService>(httpClient =>
                httpClient.BaseAddress = new Uri(configuration.GetConnectionString("MyEndpoint")));
    }
}