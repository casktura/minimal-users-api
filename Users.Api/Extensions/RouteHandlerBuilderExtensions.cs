using Users.Api.Filters;

namespace Users.Api.Extensions;

internal static class RouteHandlerBuilderExtensions
{
    public static RouteHandlerBuilder Validate<T>(this RouteHandlerBuilder routeHandlerBuilder)
    {
        return routeHandlerBuilder.AddEndpointFilter<ValidationFilter<T>>();
    }
}
