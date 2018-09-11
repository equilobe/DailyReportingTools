using System.Web.Http.Filters;
using Serilog;

namespace DailyReportWeb
{
    public class ExceptionHandlingAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            Log.Error(context.Exception, "Something went wrong");
        }
    }
}
