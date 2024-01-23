using System.Security.Claims;
using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using Users.Api.Extensions;
using Users.Api.Responses;
using Users.Api.Securities;
using Users.Library.Constants;
using Users.Library.Database;
using Users.Library.Models;
using Users.Library.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container. Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder
    .Services
    .AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new()
        {
            Title = "Minimal User API",
            Version = "v1"
        });
        options.AddSecurityDefinition("JWT", new()
        {
            In = ParameterLocation.Header,
            Description = "Please enter a valid token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = JwtBearerDefaults.AuthenticationScheme
        });
        options.AddSecurityRequirement(new()
        {
            {
                new()
                {
                    Reference = new()
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "JWT"
                    }
                },
                new string[]{ }
            }
        });
    });
builder
    .Services
    .AddDbContext<UsersDbContext>(options => options.UseMySql(Configurations.DockerConnectionString, ServerVersion.AutoDetect(Configurations.DockerConnectionString)))
    .AddScoped<UserService>();
builder
    .Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new()
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configurations.Secret)),
        };
    });
builder
    .Services
    .AddAuthorization(options => options.AddPolicy("admin", policy => policy.RequireRole(Role.Admin)));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    _ = app.UseSwagger();
    _ = app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("login", async (Login login, UserService userService) =>
{
    var loginResult = await userService.VerifyUserPasswordAsync(login);

    if (loginResult.Success)
    {
        return Response.Ok(TokenGenerator.GenerateToken(loginResult.User));
    }

    return Response.Bad();
});

var usersRouteGroup = app.MapGroup("users");

usersRouteGroup.MapPost("", async (CreateUser createUser, UserService service) => Response.Ok(await service.CreateUserAsync(createUser)));

usersRouteGroup
    .MapGet("", async (string? search, UserService service) => Response.Ok(await service.FilterUsersAsync(null, search)))
    .RequireAuthorization("admin");

usersRouteGroup
    .MapGet("{id}", async (int id, UserService service) => Response.Ok((await service.FilterUsersAsync([id])).SingleOrDefault()))
    .RequireAuthorization("admin");

usersRouteGroup
    .MapPut("{id}", async (int id, UpdateUser updateUser, UserService service) => Response.Ok(await service.UpdateUserAsync(id, updateUser)))
    .RequireAuthorization("admin");

usersRouteGroup
    .MapPut("{id}/password", async (int id, UpdateUserPassword updateUserPassword, UserService service) => Response.Ok(await service.UpdateUserPasswordAsync(id, updateUserPassword)))
    .RequireAuthorization("admin");

usersRouteGroup
    .MapDelete("{id}", async (int id, UserService service) => Response.Ok(await service.DeleteUserAsync(id)))
    .RequireAuthorization("admin");

var meRouteGroup = usersRouteGroup.MapGroup("me");

meRouteGroup
    .MapGet("", async (ClaimsPrincipal user, UserService service) => Response.Ok((await service.FilterUsersAsync([user.UserId()])).SingleOrDefault()))
    .RequireAuthorization();

meRouteGroup
    .MapPut("", async (ClaimsPrincipal user, UpdateUser updateUser, UserService service) => Response.Ok(await service.UpdateUserAsync(user.UserId(), updateUser)))
    .RequireAuthorization();

meRouteGroup
    .MapPut("password", async (ClaimsPrincipal user, UpdateUserPassword updateUserPassword, UserService service) => Response.Ok(await service.UpdateUserPasswordAsync(user.UserId(), updateUserPassword)))
    .RequireAuthorization();

meRouteGroup
    .MapDelete("", async (ClaimsPrincipal user, UserService service) => Response.Ok(await service.DeleteUserAsync(user.UserId())))
    .RequireAuthorization();

app.Run();
