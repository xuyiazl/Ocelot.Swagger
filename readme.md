
### Ocelot + Consul 微服务合并swagger文档

如果你只使用Ocelot 那么请移步[OcelotSwagger](https://github.com/Rwing/OcelotSwagger)本文是在该项目后进行了大重构。

- 解决微服务swagger文档归类合并的问题
- 解决合并后无法在swagger中直接请求问题
- 解决自定义header Authorize传参的问题

#### 思路就是：
    从微服务中拉取swagger.json，并利用中间件对swagger内的内容进行替换。
    因为在拉取到的swagger配置是针对微服务当前域的，所以在网关内是无法直接请求地址的。
    我们需要将swagger里的请求地址批量替换为上游地址。

比如微服务地址上下游配置：

```json
"DownstreamPathTemplate": "/api/{url}", //下游转发配置
"UpstreamPathTemplate": "/news/{url}", //上游路径配置
```

swagger文档内是下游地址，那么我们需要将下游地址替换为上游地址，才可以正常请求api。

![avatar](http://www.3624091.com/github/1.png)
![avatar](http://www.3624091.com/github/2.png)
![avatar](http://www.3624091.com/github/3.png)

#### 使用方式

StartUp.cs

```csharp
 public void ConfigureServices(IServiceCollection services)
 {
	
    // Load options from appsettings.json
    services.Configure<OcelotSwaggerOptions>(Configuration.GetSection(nameof(OcelotSwaggerOptions)));
    services.AddOcelotSwagger();

    services
        .AddOcelot()//添加网关
        .AddConsul()//添加consul注册发现
        .AddPolly();//添加熔断处理
 }
 
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    //开发环境不允许有swagger
    if (!env.IsEnvironment("release"))
    {
        app.UseOcelotSwagger();
    }

    app.UseOcelot().Wait();
}
```

Program.cs

```csharp
 public class Program
 {
    return WebHost.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((hostingContext, config) =>
        {
            config
                .SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
                .AddJsonFile($"appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"ocelotswagger.json", optional: true, reloadOnChange: true)
                .AddOcelot("ocelot", hostingContext.HostingEnvironment)
                .AddEnvironmentVariables()
                .AddCommandLine(args);
        })

        ......
}
```

ocelot配置文件

```json
{
  //ocelot 16+ 以下使用 ReRoutes   16+以上改为 Routes
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/{url}", //下游转发配置
      "UpstreamPathTemplate": "/news/{url}", //上游路径配置
      //"DownstreamPathTemplate": "/", //下游转发配置
      //"UpstreamPathTemplate": "/", //上游路径配置
      "UpstreamHttpMethod": [ "Get", "Post", "Delete", "Put", "Options" ],
      "UseServiceDiscovery": true, //使用服务发现
      "RouteIsCaseSensitive": false, //路由区分大小写配置，默认是不区分大小写
      //"DownstreamScheme": "http",
      //"DownstreamHostAndPorts": [
      //  {
      //    "Host": "localhost",
      //    "Port": 5005
      //  }
      //],
      //依赖polly组件
      "QosOptions": {
        "ExceptionsAllowedBeforeBreaking": 5,
        "DurationOfBreak": 10,
        "TimeoutValue": 5000
      },
      "HttpHandlerOptions": {
        "AllowAutoRedirect": false,
        "UseCookieContainer": false,
        "UseTracing": false
        //"MaxConnectionsPerServer": 100
      },
      "ServiceName": "NewsApi", //consul 服务中NewsApi的名称
      //节点服务限流
      //"EndpointRateLimiting": {
      //  "ClientWhitelist": [], //客户端白名单
      //  "EnableRateLimiting": true, //是否启用限流
      //  "Period": "1s", //限流标识时间段
      //  "PeriodTimespan": 10, //限流时长
      //  "Limit": 1 //在Period周期中最大能请求次数
      //},
      "LoadBalancerOptions": {
        "Type": "LeastConnection"
      },
      "DangerousAcceptAnyServerCertificateValidator": true //忽略ssl证书警告错误
    },
    //这里是配置微服务的swagger json文件地址，需要微服务提供
    {
      "DownstreamPathTemplate": "/{url}",
      "UpstreamPathTemplate": "/news-doc/{url}",
      "UpstreamHttpMethod": [ "Get", "Post", "Delete", "Put", "Options" ],
      "UseServiceDiscovery": true,
      "RouteIsCaseSensitive": false,
      "HttpHandlerOptions": {
        "AllowAutoRedirect": false,
        "UseCookieContainer": false,
        "UseTracing": false
        //"MaxConnectionsPerServer": 100
      },
      "ServiceName": "NewsApi",
      "LoadBalancerOptions": {
        "Type": "LeastConnection"
      },
      "DangerousAcceptAnyServerCertificateValidator": true //忽略ssl证书警告错误
    }
  ]
}

```

ocelotswagger.json 配置文件

```json
{
  "OcelotSwaggerOptions": {
    //目录是为了将下游的swagger请求地址，批量替换成网关上游地址
    "SwaggerReplaces": [
      {
        //下游路由正则（swagger.json内需要替换的内容正则）
        "DownstreamPathRouteRegex": "\\/api\\/",
        //上游路由正则（从上游请求地址中获取上游目录，方便在swagger中直接请求）
        "UpstreamPathRouteRegex": "\\/(.*?)\\-doc\\/"
      }
    ],
    "SwaggerEndpoints": [
      {
        //swagger分类的名称
        "Name": "[news]-financenews-v1 api",
        //swagger json上游地址
        "Url": "/news-doc/swagger/financenews-v1/swagger.json"
      },
      {
        "Name": "[news]-http-v1 api",
        "Url": "/news-doc/swagger/http-v1/swagger.json"
      },
      {
        "Name": "[news]-script-v1 api",
        "Url": "/news-doc/swagger/script-v1/swagger.json"
      }
    ]
  }
}

```

如果需要自定义token或者自定义header参数，需要在微服务的swagger里配置

```csharp
services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("appid", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Name = "appid",
            Description = "在下框中输入appid",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "appid"
        });

        options.AddSecurityDefinition("token", new OpenApiSecurityScheme
        {
            Description = "在下框中输入授权token",
            In = ParameterLocation.Header,
            Name = "token",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "string"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference {
                            Type = ReferenceType.SecurityScheme,
                            Id = "appid"
                        }
                    },
                    new string[] { }
                },
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference {
                            Type = ReferenceType.SecurityScheme,
                            Id = "token"
                        }
                    },
                    new string[] { }
                }
            });
    });
```
