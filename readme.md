# Service annotations

Attributes for automatic registration in ServiceCollection

##### dotnet add package [ServiceAnnotations](https://www.nuget.org/packages/ServiceAnnotations/)


## Usage examples

### ServiceAttribute

The attribute instructs assembly scanner to register class it is applied to 
as implementation type for given types or itself if not types specified.

The attribute can be applied on classes with any access modifier (public/internal/private/protected).

```c#
[Service(ServiceLifetime.Transient)]
class MyService
{
    // the class will be registered with Transient lifetime as MyService

    public MyService(IMyServiceDependency repository) { }
}

[Service(ServiceLifetime.Scoped, typeof(IMyServiceDependency))]
class MyServiceDependency 
{ 
    // the class will be registered with Scoped lifetime as IMyServiceDependency
}

public class StartUp
{
    public static void ConfigureServices(IServiceCollection serviceCollection)
    {
        // to add all services from calling assembly (the assembly where this code is written, typically the aspent app) 
        serviceCollection.AddAnnotatedServices();
        
        // too add all services from specific assembly
        serviceCollection.AddAnnotatedServices(typeof(AnyTypeFromTargetAssemly).Assembly);
    }
}
```

### ServiceAttribute multiple types

```c#
[Service(ServiceLifetime.Scoped, typeof(ServiceWithManyHats), typeof(IRedHat), typeof(IGreenHat)]
class ServiceWithManyHats : IRedHat, IBlueHat, IGreenHat {
}
```
the type above will get registered in a following manner
```c#
// primary registration -> the service will get registered as Type and as Implementation
serviceCollection.AddScoped<ServiceWithManyHats, ServiceWithManyHats>();

// secondary registrations -> all other types will get registered as resolves for primary registration
serviceCollection.AddScoped<IRedHat>(serviceProvider => (IRedHat)serviceProvider.Resolve(typeof(ServiceWithManyHats)); 
serviceCollection.AddScoped<IGreenHat>(serviceProvider => (IGreenHat)serviceProvider.Resolve(typeof(ServiceWithManyHats));

// The IBlueHat wasn't mentioned in attribute, therefore won't be registered
```
> with Singleton or Scoped lifetime (withing single lifetime scope) 
> resolving service by all three keys will result the same instance

#### How primary type is selected

If type of a class is in the list of types it always becomes primary,
otherwise the first in list.

In the example above is first in a list of types `typeof(ServiceWithManyHats)`, making it last in a list would change nothing. 

However removing `typeof(ServiceWithManyHats)` from the list would change the registration manner to following:

```c#
// primary registration -> the service will get registered as Type and as Implementation
serviceCollection.AddScoped<IRedHat, ServiceWithManyHats>();

// secondary registration -> all other types will get registered as resolves for primary registration
serviceCollection.AddScoped<IGreenHat>(serviceProvider => (IGreenHat)serviceProvider.Resolve(typeof(IRedHat)));
```

### ConfigureServicesAttribute

Configure services attribute instructs assembly scanner to invoke static method 
ConfigureServices (or custom name if given in attribute) of a class it's applied on.

The attribute can point to method with any access modifier (public/private/internal), but method must be static.
It doesn't matter if class is static or not.

*very basic example*
```c#
[ConfigureServices]
static class MyModule {
    static ConfigureServices(IServiceCollection serviceCollection) {
        serviceCollection.AddTransient<MyService>();
        serviceCollection.AddTransient<MyOtherService>();
    }
}
```

### ConfigureServicesAttribute with additional parameters on ConfigureServices methods

*practical example with HttpClient*
```c#
[Service(ServiceLifetime.Transient), ConfigureServices("RegisterHttpClient")]
class MyService {
    
    public MyService(HttpClient myHttpClient) { }
    
    static void RegisterHttpClient(IServiceCollection serviceColleciton, IConfiguration configuration) {
        serviceCollection.AddHttpClient<MyService>(httpClient => {
            httpClient.BaseUrl = new Uri(configuration.GetConnectionString("myServiceEndpoint")
        })
    }
    
}

// please note
// an example passing IConfiguration or any other service to assembly scanner in classic aspnet core app

public class Startup
{
    readonly IConfiguration _configuration;
    readonly IWebHostingEnvironment _environment;
    
    public Startup(IConfiguration configuration, IWebHostingEnvironment _environment)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // Pass the objects that will be avialble as parameters in ConfigureServices method(s) invoked by ConfigureServicesAttribute 
        services.AddAnnotatedServices(context => context
            .Add<IConfiguration>(_configuration)
            .Add<IWebHostingEnvironment>(_environment));
    }

    public void Configure(IApplicationBuilder app)
    {
        // ...
    }
}
```


## Errors

### SA2000

#### Configure services method not found by name

**Case #1**: ConfigureServicesAttribute by default looks by method named ConfigureServices, but method is not present in class
```c#
[ConfigureServices]
class MyClass {
    static void RegisterServices(IServiceCollection serviceCollection) {}
}
```
**Case #1 Solution #1**: Make sure the method is called ConfigureServices
```c#
[ConfigureServices]
class MyClass {
    static void ConfigureServices(IServiceCollection serviceCollection) {}
}
```
**Case #1 Solution #2**: Specify method name in ConfigureServicesAttribute
```c#
[ConfigureServices("RegisterServices")]
class MyClass {
    static void RegisterServices(IServiceCollection serviceCollection) {}
}
```

**Case #2**: ConfigureServicesAttribute looks for static method RegisterServices, but the method defined in class is not static.
```c#
[ConfigureServices("RegisterServices")]
class MyClass {
    void RegisterServices(IServiceCollection serviceCollection) {}
}
```

**Case #2 Solution**: Ensure method is static
```c#
[ConfigureServices("RegisterServices")]
class MyClass {
    void RegisterServices(IServiceCollection serviceCollection) {}
}
```

### SA2001

#### Ambiguous configure services method name

**Case:** ConfigureServiceAttribute looks for static "ConfigureServices" (or custom name if specified) method.
There are two methods matching description. ConfigureServiceAttribute cannot decide which one to use.

```c#
[ConfigureServices]
class MyClass {
    static ConfigureServices(IServiceCollection serviceCollection) {}
    static ConfigureServices(IServiceCollection serviceCollection, IConfiguration configuration) {}
}
```

> non-static methods doesn't affect resolution, in other words the class may have endless count of non-static methods matching the same name it wont cause a problem

**Solution**: Ensure the method name referred by ConfigureServicesAttribute is unique.

```c#
[ConfigureServices]
public class MyClass {
    public static RegisterServices(IServiceCollection serviceCollection) {}
    public static ConfigureServices(IServiceCollection serviceCollection, IConfiguration configuration) {}
}
```

### SA2002
#### The instance of &lt;Type&gt; was not provided.

**Case**: ConfigureServicesAttribute looks for static "ConfigureServices" (or custom name method if specified)
the method is present, the method is static, but has more parameters than just IServiceCollection.

```c#
[ConfigureServices]
public class MyClass {
    public static ConfigureServices(IServiceCollection serviceCollection, IConfiguration configuration) {}
}
```

**Solution**: Pass instances of objects required to register services when invoking AddAnnotatedServices method,
like on example below
```c#
public class Startup
{
    readonly IConfiguration _configuration;
    
    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc();
        services.AddAnnotatedServices(context => context.Add<IConfiguration>(_configuration));
    }

    public void Configure(IApplicationBuilder app)
    {
        // ...
    }
}
```

## License

MIT