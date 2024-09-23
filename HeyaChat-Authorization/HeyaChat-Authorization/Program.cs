using HeyaChat_Authorization.Middleware;
using HeyaChat_Authorization.Models.Context;
using HeyaChat_Authorization.Repositories;
using HeyaChat_Authorization.Repositories.Interfaces;
using HeyaChat_Authorization.Services;
using HeyaChat_Authorization.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Filters;

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine($"Current environment: {builder.Environment.EnvironmentName}");

// Read config values with repository
var _config = builder.Configuration;
var _repository = new ConfigurationRepository(_config);

builder.Services.AddControllers();
builder.Services.AddDbContext<AuthorizationDBContext>();
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
        ValidIssuer = _repository.GetIssuer(),
        ValidAudience = _repository.GetAudience(),
        IssuerSigningKey = new SymmetricSecurityKey(_repository.GetSigningKey()),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
    };
});

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => 
{
    options.AddSecurityDefinition("Bearer token", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
    });
    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

// Services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IHasherService, HasherService>();

// Repositories
builder.Services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<IUserDetailsRepository, UserDetailsRepository>();
builder.Services.AddScoped<IDevicesRepository, DevicesRepository>();
builder.Services.AddScoped<ICodesRepository, CodesRepository>();
builder.Services.AddScoped<ITokensRepository, TokensRepository>();
builder.Services.AddScoped<ISuspensionsRepository, SuspensionsRepository>();
builder.Services.AddScoped<IAuditLogsRepository, AuditLogsRepository>();

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

app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseMiddleware<AuthorizeHeaderMiddleware>();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();