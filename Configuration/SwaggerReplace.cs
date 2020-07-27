namespace Ocelot.Swagger.Configuration
{
    /// <summary>
    /// 目录是为了将下游swagger生成的json请求地址，批量替换成网关上游地址
    /// </summary>
    public class SwaggerReplace
    {
        /// <summary>
        /// 下游路由正则（swagger.json内需要替换的内容正则）
        /// </summary>
        public string DownstreamPathRouteRegex { get; set; }
        /// <summary>
        /// 上游路由正则（从上游请求地址中获取上游目录，方便在swagger中直接请求）
        /// </summary>
        public string UpstreamPathRouteRegex { get; set; }
    }
}