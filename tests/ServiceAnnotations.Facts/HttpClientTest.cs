using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ServiceAnnotations.Tests
{
    public class HttpClientTest
    {
        [Fact]
        public void Foo()
        {
            var svc = new ServiceCollection();
            svc.AddAnnotatedServices();

            var sp = svc.BuildServiceProvider();

            var myService = sp.GetService<MyService>();
        }
    }

    [Service(ServiceLifetime.Transient), ConfigureServices]
    public class MyService
    {
        public readonly HttpClient HttpClient;
        
        public MyService(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddHttpClient<MyService>(client =>
            {
                client.BaseAddress = new Uri("https://www.google.com");
            });
        }
    }
}