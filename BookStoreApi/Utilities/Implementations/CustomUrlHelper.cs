using Microsoft.AspNetCore.Mvc;

namespace BookStoreApi.Utilities;

public class CustomUrlHelper : ICustomUrlHelper
{
    private readonly IUrlHelper _urlHelper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CustomUrlHelper(IUrlHelper urlHelper, IHttpContextAccessor httpContextAccessor)
    {
        _urlHelper = urlHelper;
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetScheme()
    {
        string scheme = _urlHelper.ActionContext.HttpContext.Request.Scheme;
        string host = _urlHelper.ActionContext.HttpContext.Request.Host.Host;
        if (host != "localhost" && host != "127.0.0.1")
        {
            scheme = "https";
        }
        return scheme;
    }

    public string? GeneratePageLink(string action, string controller, int targetPage, int pageSize)
    {
        string scheme = GetScheme();
        return _urlHelper.Action(action, controller, new { PageNumber = targetPage, PageSize = pageSize }, scheme);
    }
}

