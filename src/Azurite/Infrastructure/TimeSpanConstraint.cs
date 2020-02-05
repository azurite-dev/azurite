using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Azurite.Infrastructure
{
    public class TimeSpanConstraint : IRouteConstraint
    {
        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (httpContext == null)  
                throw new ArgumentNullException(nameof(httpContext));  
  
            if (route == null)  
                throw new ArgumentNullException(nameof(route));  
  
            if (routeKey == null)  
                throw new ArgumentNullException(nameof(routeKey));  
  
            if (values == null)  
                throw new ArgumentNullException(nameof(values));  
  
            object routeValue;  
  
            if(values.TryGetValue(routeKey, out routeValue))  
            {
                var span = routeValue.ToString();
                return TimeSpan.TryParse(span, out TimeSpan tSpan);
            }  
  
            return false;
        }
    }
}