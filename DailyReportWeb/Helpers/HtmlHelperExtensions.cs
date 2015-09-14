using DailyReportWeb;

namespace System.Web.Mvc
{
    public static class HtmlHelperExtensions
    {
        public static MvcHtmlString IncludeConnectJs<T>(this HtmlHelper<T> htmlHelper)
        {
            //if (!htmlHelper.ViewContext.HttpContext.User.IsPlugin())
            //    return null;

            if (htmlHelper.ViewContext.HttpContext.Request.UrlReferrer == null || !htmlHelper.ViewContext.HttpContext.Request.UrlReferrer.AbsoluteUri.ToLower().Contains("atlassian"))
                return null;

            var requestQueryString = htmlHelper.ViewContext.HttpContext.Request.QueryString;
            var baseUrl = requestQueryString["xdm_e"] + requestQueryString["cp"];

            return MvcHtmlString.Create("<script type='text/javascript' data-options='sizeToParent:true' src='" + baseUrl + "/atlassian-connect/all.js'></script>");
        }
    }
}
