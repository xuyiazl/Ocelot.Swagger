namespace Ocelot.Swagger.Configuration
{
    using System.Collections.Generic;

    public class OcelotSwaggerOptions
    {
        public List<SwaggerReplace> SwaggerReplaces { get; set; } = new List<SwaggerReplace>();
        public List<SwaggerEndPoint> SwaggerEndPoints { get; set; } = new List<SwaggerEndPoint>();
    }
}