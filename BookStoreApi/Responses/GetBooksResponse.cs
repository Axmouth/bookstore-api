using BookStoreApi.Models;
using Microsoft.AspNetCore.Mvc;
using BookStoreApi.Controllers;

namespace BookStoreApi.Responses;

public class GetBooksResponse
{
    public required GetBookResponse[] Books { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalItems { get; set; }

    public string? NextPage { get; set; }

    public string? PreviousPage { get; set; }

    public static GetBooksResponse FromBooks(Book[] books, int pageNumber, int pageSize, int total, IUrlHelper urlHelper)
    {
        var totalPages = (int)Math.Ceiling((double)total / pageSize);

        return new GetBooksResponse
        {
            Books = books.Select(GetBookResponse.FromBook).ToArray(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = total,
            NextPage = pageNumber < totalPages ? urlHelper.Link(default, new { PageNumber = pageNumber + 1, PageSize = pageSize }) : null,
            PreviousPage = pageNumber > 1 ? urlHelper.Link(default, new { PageNumber = pageNumber - 1, PageSize = pageSize }) : null
        };
    }
}

