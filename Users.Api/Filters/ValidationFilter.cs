using System.ComponentModel.DataAnnotations;

using Users.Api.Responses;

namespace Users.Api.Filters;

internal class ValidationFilter<T> : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var model = context.GetArgument<T>(0);

        if (model != null)
        {
            var validationContext = new ValidationContext(model);
            var validationResults = new List<ValidationResult>();
            bool valid = Validator.TryValidateObject(model, validationContext, validationResults);

            if (!valid)
            {
                return Response.InvalidModel(validationResults);
            }
        }

        return await next(context);
    }
}
