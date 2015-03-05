// namespace used so you get this extention magically in the Razor templates
namespace System.Web.Mvc
{
    public static class HtmlHelperExtensions
    {
        public static MvcHtmlString IncludeConnectJs<T>(this HtmlHelper<T> htmlHelper)
        {
            var xdm = HttpUtility.HtmlDecode(htmlHelper.ViewContext.HttpContext.Request.QueryString.Get("xdm_e"));
            var cp = HttpUtility.HtmlDecode(htmlHelper.ViewContext.HttpContext.Request.QueryString.Get("cp"));

            return MvcHtmlString.Create("<script type='text/javascript' src='" + xdm + cp + "/atlassian-connect/all.js'></script>");
        }
    }
}
