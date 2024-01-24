namespace Users.Api.Responses;

public class BadResponse : Response
{
}

public class BadResponse<T> : Response
{
    public string ErrorCode { get; set; }

    public T Error { get; set; }

    public BadResponse(string errorCode, T error)
    {
        ErrorCode = errorCode;
        Error = error;
    }
}
