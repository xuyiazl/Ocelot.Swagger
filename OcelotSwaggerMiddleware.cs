namespace Ocelot.Swagger
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;

    using Ocelot.Swagger.Configuration;

    internal class OcelotSwaggerMiddleware
    {
        private readonly OcelotSwaggerOptions _options;

        private readonly RequestDelegate _next;

        public OcelotSwaggerMiddleware(
            RequestDelegate next,
            IOptionsMonitor<OcelotSwaggerOptions> optionsAccessor)
        {
            this._next = next;
            this._options = optionsAccessor.CurrentValue;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            var path = httpContext.Request.Path.Value;

            if (path.IndexOf("/swagger") > -1)
            {
                var content = await this.ReadContentAsync(httpContext);

                foreach (var replace in _options.SwaggerReplaces)
                {
                    Regex reg = new Regex(replace.UpstreamPathRouteRegex);

                    Match match = reg.Match(path);

                    if (match.Success)
                    {
                        string value = match.Groups[1].Value;

                        content = Regex.Replace(content, replace.DownstreamPathRouteRegex, $"/{value}/");
                    }
                }

                await this.WriteContentAsync(httpContext, content);
            }
            else
            {
                await this._next(httpContext);
            }
        }

        private async Task<string> ReadContentAsync([NotNull] HttpContext httpContext)
        {
            var existingBody = httpContext.Response.Body;
            using (var newBody = new MemoryStream())
            {
                // We set the response body to our stream so we can read after the chain of middlewares have been called.
                httpContext.Response.Body = newBody;

                await this._next(httpContext);

                // Reset the body so nothing from the latter middlewares goes to the output.
                httpContext.Response.Body = existingBody;

                newBody.Seek(0, SeekOrigin.Begin);
                var newContent = await new StreamReader(newBody).ReadToEndAsync();

                return newContent;
            }
        }

        private async Task WriteContentAsync([NotNull] HttpContext httpContext, string content)
        {
            httpContext.Response.ContentLength = Encoding.UTF8.GetByteCount(content);
            await httpContext.Response.WriteAsync(content);
        }
    }
}