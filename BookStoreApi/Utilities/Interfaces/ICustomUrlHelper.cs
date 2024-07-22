using Microsoft.AspNetCore.Mvc;

namespace BookStoreApi.Utilities;

public interface ICustomUrlHelper
{
    string? GeneratePageLink(string actionName, string controllerName, int targetPage, int pageSize);
    string GetScheme();
}

