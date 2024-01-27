using ErrorOr;

namespace WineMate.Reviews.Extensions;

public static class ErrorExtensions
{
    public static IResult ToResponse(this Error error)
    {
        return ToResponse(new List<Error> { error });
    }

    public static IResult ToResponse(this List<Error> errors)
    {
        var firstError = errors[0];

        var statusCode = GetStatusCode(firstError);
        var title = GetTitle(firstError);
        var type = GetType(firstError);

        return Results.Problem(
            statusCode: statusCode,
            title: title,
            type: type,
            extensions: new Dictionary<string, object?>
            {
                ["errors"] = errors.Select(error => new { error.Code, Message = error.Description })
            }
        );
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

    private static string GetTitle(Error error)
    {
        return error.Type switch
        {
            ErrorType.Failure => "Failure",
            ErrorType.Validation => "Validation",
            ErrorType.Unauthorized => "Unauthorized",
            ErrorType.NotFound => "Not Found",
            ErrorType.Conflict => "Conflict",
            _ => "Internal Server Error"
        };
    }

    private static string? GetType(Error error)
    {
        return error.Type switch
        {
            ErrorType.Failure => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
            ErrorType.Validation => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
            ErrorType.Unauthorized => null,
            ErrorType.NotFound => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
            ErrorType.Conflict => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8",
            _ => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1"
        };
    }
}
