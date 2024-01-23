using System.Collections;

namespace Users.Api.Responses;

public class OkResponse : Response
{
    public OkResponse()
    {
        Success = true;
    }
}

public class OkResponse<T> : Response
{
    public T? Result { get; set; }

    public int? Total { get; set; } = null;

    public OkResponse(T result)
    {
        Success = true;
        Result = result;

        if (result is ICollection collection)
        {
            Total = collection.Count;
        }
    }
}
