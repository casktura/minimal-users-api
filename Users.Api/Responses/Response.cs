using System.ComponentModel.DataAnnotations;

namespace Users.Api.Responses;

public abstract class Response
{
    public bool Success { get; set; }

    public static IResult Ok()
    {
        return Results.Ok(new OkResponse());
    }

    public static IResult Ok<T>(T result)
    {
        return Results.Ok(new OkResponse<T>(result));
    }

    public static IResult Bad()
    {
        return Results.Ok(new BadResponse());
    }

    public static IResult InvalidModel(ICollection<ValidationResult> validationResults)
    {
        return Results.Ok(new BadResponse<ICollection<ValidationResult>>("ERR_INVALID_MODEL", validationResults));
    }
}
