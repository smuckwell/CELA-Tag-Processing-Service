using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace CELA_Tags_Parsing_Service
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            //See https://stackoverflow.com/questions/44920319/the-request-entitys-media-type-text-plain-is-not-supported-for-this-resource for explanation
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain"));
            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
