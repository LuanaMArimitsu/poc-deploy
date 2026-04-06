using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebsupplyConnect.API.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class ApiKeyAuthAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context) 
        {
            var apiKey = context.HttpContext.Request.Headers["x-api-key"].FirstOrDefault();

            var config = context.HttpContext.RequestServices.GetService(typeof(IConfiguration)) as IConfiguration;
            var expectedApiKey = config?.GetValue<string>("APIsConnect:ApiKey");

            if (string.IsNullOrEmpty(apiKey) || apiKey != expectedApiKey)
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
