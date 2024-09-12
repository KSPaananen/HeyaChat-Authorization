using HeyaChat_Authorization.Repositories.Configuration;
using HeyaChat_Authorization.Repositories.Users;
using HeyaChat_Authorization.Repositories.Users.Interfaces;
using HeyaChat_Authorization.Services;
using HeyaChat_Authorization.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Filters;
using System.Text;

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine($"Current environment: {builder.Environment.EnvironmentName}");

// Read config values with repository
var config = builder.Configuration;
var repository = new ConfigurationRepository(config);

builder.Services.AddControllers();
// builder.Services.AddDbContext<Name>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", options =>
    {
        options.AllowCredentials();
        options.SetIsOriginAllowed(org => new Uri(org).Host == "localhost");
        options.WithExposedHeaders("Authorization"); // Define custom headers here
        options.AllowAnyHeader();
        options.AllowAnyMethod();
    });
    options.AddPolicy("Production", options =>
    {   // Configure these later
        options.WithExposedHeaders("Authorization");
        options.WithMethods("POST", "PUT", "GET", "DELETE");
        options.WithOrigins("");
        options.AllowAnyHeader();
    });
});
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
#if DEBUG
    options.IncludeErrorDetails = true;
#else
    options.RequireHttpsMetadata = false;
#endif

    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = repository.GetIssuerFromConfiguration(),
        ValidAudience = repository.GetAudienceFromConfiguration(),
        IssuerSigningKey = new SymmetricSecurityKey(repository.GetSigningKeyFromConfiguration()),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
    };
});

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => // This swagger configuration doesn't work in .NET 8. Worked in .NET 7 projects??
{
    options.AddSecurityDefinition("Bearer token", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
    });
    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

// Add services 
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Add repositories
builder.Services.AddScoped<IUsersRepository, UsersRepository>();

// Define ports for enviroments
if (builder.Environment.IsDevelopment())
{
    builder.WebHost.UseUrls("http://*:7000", "https://*:7001");
}
else
{
    builder.WebHost.UseUrls("http://*:8000", "https://*:8001");
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("Development");
}
else
{
    app.UseCors("Production");
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();