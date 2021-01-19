# Service annotations

Attributes for automatic registration in ServiceCollection

*usage example*


```c#
[Service(ServiceLifetime.Scoped, typeof(IRepository))]
public class Repository { }

[Service(ServiceLifetime.Singleton)]
public class Service
{
    public Service(IRepository repository) { }
}
```

```c#
public class StartUp
{
    public static void ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddAnnotatedServices();
    }
}
```

Complete reference will follow soon

### License

MIT