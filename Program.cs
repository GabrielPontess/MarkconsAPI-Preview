using Markcons.Auth;
using Markcons.Extensions;
using Markcons.Models;
using Markcons.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

#region Builder

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });


builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Markcons API",
        Description = "Developed by Gabriel Pontes",
        Contact = new OpenApiContact { Name = "Catos Computer Club", Email = "catoscomputerclub@gmail.com" },
        License = new OpenApiLicense { Name = "MIT", Url = new Uri("https://opensource.org/licenses/MIT") }
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Insira o token JWT desta maneira: Bearer {seu token}",
        Name = "Authorization",
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
{
    var connectionString = builder.Configuration.GetValue<string>("ConnectionStrings:MongoDbConnection");
    return new MongoClient(connectionString);
});
builder.Services.AddSingleton<MarkService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

#endregion

#region Configure App

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

MapRoutes(app);
app.Run();

#endregion

#region Routes

/* Exemplo */
void MapRoutes(WebApplication app)
{
    app.MapPost("/Login", [AllowAnonymous] (LoginRequest request) =>
    {
        if (request.Username == "admin" && request.Password == "admin")
        {
            var token = JwtAuthentication.GenerateJwtToken(request.Username);
            return Results.Ok(new { Token = token });
        }

        return Results.Unauthorized();
    })
    .Produces<object>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status400BadRequest)
    .WithName("Login")
    .WithTags("Auth")
    .WithOpenApi(); ;


    app.MapPost("/CreateMarkdown", [Authorize] async (MarkService mongoDbService, [FromBody] MarkdownFile markdown) =>
    {
        var response = await mongoDbService.CreateAsync(markdown);

        if (!response.IsSuccess)
        {
            return Results.BadRequest(response.Error);
        }

        return Results.Ok(response);
    })
    .Produces<ErroOr<MarkdownFile>>(StatusCodes.Status200OK)
    .Produces<string>(StatusCodes.Status400BadRequest)
    .WithName("CreateMarkdown")
    .WithTags("Markdowns")
    .WithOpenApi();

    app.MapGet("/GetMarkdowns", [Authorize] async (MarkService mongoDbService) =>
    {
        var response = await mongoDbService.GetAllAsync();

        if (!response.IsSuccess)
        {
            return Results.BadRequest(response.Error);
        }

        return Results.Ok(response);
    })
    .Produces<ErroOr<List<MarkdownFile>>>(StatusCodes.Status200OK)
    .Produces<string>(StatusCodes.Status400BadRequest)
    .WithName("GetMarkdowns")
    .WithTags("Markdowns")
    .RequireAuthorization()
    .WithOpenApi();

    app.MapGet("/GetMarkdownsById/", [Authorize] async (MarkService mongoDbService, string id) =>
    {
        var response = await mongoDbService.GetByIdAsync(id);

        if (!response.IsSuccess)
        {
            return Results.BadRequest(response.Error);
        }

        return Results.Ok(response);
    })
    .Produces<ErroOr<MarkdownFile>>(StatusCodes.Status200OK)
    .Produces<string>(StatusCodes.Status400BadRequest)
    .WithName("GetMarkdownsById")
    .WithTags("Markdowns")
    .RequireAuthorization()
    .WithOpenApi();

    app.MapGet("/GetMarkdownsByPrefix/{prefix}/", [Authorize] async (MarkService mongoDbService,string prefix) =>
    {
        var response = await mongoDbService.GetByPrefixAsync(prefix);

        if (!response.IsSuccess)
        {
            return Results.BadRequest(response.Error);
        }

        return Results.Ok(response);
    })
    .Produces<ErroOr<List<MarkdownFile>>>(StatusCodes.Status200OK)
    .Produces<string>(StatusCodes.Status400BadRequest)
    .WithName("GetMarkdownsByPrefix")
    .WithTags("Markdowns")
    .RequireAuthorization()
    .WithOpenApi();

    app.MapGet("/GetMarkdownsByTitle/{title}/", [Authorize] async (MarkService mongoDbService, string title) =>
    {
        var response = await mongoDbService.GetByTitleAsync(title);

        if (!response.IsSuccess)
        {
            return Results.BadRequest(response.Error);
        }

        return Results.Ok(response);
    })
   .Produces<ErroOr<List<MarkdownFile>>>(StatusCodes.Status200OK)
   .Produces<string>(StatusCodes.Status400BadRequest)
   .WithName("GetMarkdownsByTitle")
   .WithTags("Markdowns")
   .RequireAuthorization()
   .WithOpenApi();


    app.MapPut("/UpdateMarkdown/", [Authorize] async (MarkService mongoDbService, string id,[FromBody] MarkdownFile markdown) =>
    {
        var response = await mongoDbService.UpdateAsync(id, markdown);

        if (!response.IsSuccess)
        {
            return Results.BadRequest(response.Error);
        }

        return Results.Ok(response);
    })
   .Produces<ErroOr<UpdateResult>>(StatusCodes.Status200OK)
   .Produces<string>(StatusCodes.Status400BadRequest)
   .WithName("UpdateMarkdown")
   .WithTags("Markdowns")
   .RequireAuthorization()
   .WithOpenApi();

    app.MapDelete("/DeleteMarkdown/", [Authorize] async (MarkService mongoDbService, string id) =>
    {
        var response = await mongoDbService.DeleteAsync(id);

        if (!response.IsSuccess)
        {
            return Results.BadRequest(response.Error);
        }

        return Results.Ok(response);
    })
   .Produces<ErroOr<DeleteResult>>(StatusCodes.Status200OK)
   .Produces<string>(StatusCodes.Status400BadRequest)
   .WithName("DeleteMarkdown")
   .WithTags("Markdowns")
   .RequireAuthorization()
   .WithOpenApi();
}

#endregion

