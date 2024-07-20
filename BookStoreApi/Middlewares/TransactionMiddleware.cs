using BookStoreApi.Data;

namespace BookStoreApi.Middlewares;

public class TransactionMiddleware
{
    private readonly RequestDelegate _next;

    public TransactionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
    {
        using (var transaction = dbContext.Database.BeginTransaction())
        {
            try
            {
                await _next(context);
                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;  // Ensure any exceptions are still thrown after rollback
            }
        }
    }
}

