using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Octokit;

namespace ServiceAnnotations.MvcSample
{
    public class ApiController
    {
        readonly MyService _myService;
        public ApiController(MyService myService)
        {
            _myService = myService;
        }

        [HttpGet("")]
        public string Get()
        {
            return _myService.MyDependency.ConnectionString;
        }
    }

    [Service(ServiceLifetime.Scoped), ConfigureServices]
    public class MyService
    {
        public MyDependency MyDependency;
        public MyService(MyDependency myDependency)
        {
            MyDependency = myDependency;
        }

        static void ConfigureServices(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddScoped(sp => new MyDependency
            {
                ConnectionString = configuration.GetConnectionString("MyDependency")
            });
        }
    }

    public class MyDependency
    {
        public string ConnectionString { get; set; }
    }
}