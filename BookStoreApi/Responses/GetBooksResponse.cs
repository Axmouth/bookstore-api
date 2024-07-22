using BookStoreApi.Models;
using BookStoreApi.Controllers;
using BookStoreApi.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace BookStoreApi.Responses;

public class GetBooksResponse
{
    public required GetBookResponse[] Books { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalItems { get; set; }

    public string? NextPage { get; set; }

    public string? PreviousPage { get; set; }

    public static GetBooksResponse FromBooks(Book[] books, int pageNumber, int pageSize, int total, ICustomUrlHelper customUrlHelper)
    {
        var totalPages = (int)Math.Ceiling((double)total / pageSize);
        string controllerName = nameof(BooksController).Replace("Controller", "");

        var nextPage = pageNumber < totalPages ? customUrlHelper.GeneratePageLink(nameof(BooksController.GetBooks), controllerName, pageNumber + 1, pageSize) : null;
        var previousPage = pageNumber > 1 ? customUrlHelper.GeneratePageLink(nameof(BooksController.GetBooks), controllerName, pageNumber - 1, pageSize) : null;

        return new GetBooksResponse
        {
            Books = books.Select(GetBookResponse.FromBook).ToArray(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = total,
            NextPage = nextPage,
            PreviousPage = previousPage
        };
    }
}
