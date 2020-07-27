namespace Ocelot.Swagger.Extensions
{
    using System;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OpenApi.Models;
    using Ocelot.Swagger.Configuration;

    public static class MvcServiceCollectionExtensions
    {
        public static IServiceCollection AddOcelotSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                //options.AddSecurityDefinition("ocgw-client-appid", new OpenApiSecurityScheme
                //{
                //    In = ParameterLocation.Header,
                //    Name = "ocgw-client-appid",
                //    Description = "在下框中输入appid",
                //    Type = SecuritySchemeType.ApiKey,
                //    Scheme = "ocgw-client-appid"
                //});

                //options.AddSecurityDefinition("ocgw-client-token", new OpenApiSecurityScheme
                //{
                //    Description = "在下框中输入授权token",
                //    In = ParameterLocation.Header,
                //    Name = "ocgw-client-token",
                //    Type = SecuritySchemeType.ApiKey,
                //    Scheme = "string"
                //});

                //options.AddSecurityRequirement(new OpenApiSecurityRequirement
                //    {
                //        {
                //            new OpenApiSecurityScheme
                //            {
                //                Reference = new OpenApiReference {
                //                    Type = ReferenceType.SecurityScheme,
                //                    Id = "ocgw-client-appid"
                //                }
                //            },
                //            new string[] { }
                //        },
                //        {
                //            new OpenApiSecurityScheme
                //            {
                //                Reference = new OpenApiReference {
                //                    Type = ReferenceType.SecurityScheme,
                //                    Id = "ocgw-client-token"
                //                }
                //            },
                //            new string[] { }
                //        }
                //    });
            });
            return services;
        }
    }
}