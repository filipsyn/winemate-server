using ErrorOr;

namespace WineMate.Catalog.Extensions;

public static class ErrorExtensions
{
    public static IResult ToResponse(this Error error)
    {
        var statusCode = GetStatusCode(error);

        return Results.Problem(
            statusCode: statusCode,
            title: error.Code,
            detail: error.Description);
    }

    private static int GetStatusCode(Error error)
    {
        return error.Type switch
        {
            ErrorType.Failure => StatusCodes.Status400BadRequest,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };
    }
}
