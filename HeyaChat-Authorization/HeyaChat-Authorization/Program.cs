using HeyaChat_Authorization.Middleware;
using HeyaChat_Authorization.Models.Context;
using HeyaChat_Authorization.Repositories;
using HeyaChat_Authorization.Repositories.Interfaces;
using HeyaChat_Authorization.Services;
using HeyaChat_Authorization.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Filters;
using System.Security.Cryptography.X509Certificates;
using System.Threading.RateLimiting;

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine($"Current environment: {builder.Environment.EnvironmentName}");

// Read config values with repository
var _config = builder.Configuration;
var _configurationRepository = new ConfigurationRepository(_config);

builder.Services.AddControllers();
builder.Services.AddDbContext<AuthorizationDBContext>();
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", options =>
    {
        options.PermitLimit = _configurationRepository.GetPermitLimit();
        options.Window = _configurationRepository.GetTimeWindow();
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = _configurationRepository.GetQueueLimit();
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", options =>
    {
        options.AllowCredentials();
        options.SetIsOriginAllowed(org => new Uri(org).Host == "localhost");
        options.WithExposedHeaders("Authorization");
        options.AllowAnyHeader();
        options.AllowAnyMethod();
    });
    options.AddPolicy("Production", options =>
    {
        options.WithExposedHeaders("Authorization");
        options.WithMethods("POST", "PUT", "GET", "DELETE");
        options.AllowAnyOrigin();
        options.AllowAnyHeader();
    });
});

// Configure certificate for use with data protection
string certificatePath = _configurationRepository.GetCertificatePath() ?? throw new NullReferenceException("Certificate filepath null.");
string certificatePassword = _configurationRepository.GetCertificatePassword() ?? throw new NullReferenceException("Certificate password null.");

X509Certificate2 certificate = new X509Certificate2(certificatePath, certificatePassword);

builder.Services.AddDataProtection() // 26/9/2024 Persisting to database doesn't work with postgresql's entityframework
    .PersistKeysToFileSystem(new DirectoryInfo(_configurationRepository.GetKeyStoragePath()))
    .ProtectKeysWithCertificate(certificate)
    .SetDefaultKeyLifetime(_configurationRepository.GetAverageKeyLifetime())
    .SetApplicationName(_configurationRepository.GetApplicationName());

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
    options.IncludeErrorDetails = false;
#endif

    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = _configurationRepository.GetIssuer(),
        ValidAudience = _configurationRepository.GetAudience(),
        IssuerSigningKey = new SymmetricSecurityKey(_configurationRepository.GetSigningKey()),
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
builder.Services.AddScoped<IToolsService, ToolsService>();

// Repositories
builder.Services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<IUserDetailsRepository, UserDetailsRepository>();
builder.Services.AddScoped<IDevicesRepository, DevicesRepository>();
builder.Services.AddScoped<ICodesRepository, CodesRepository>();
builder.Services.AddScoped<ITokensRepository, TokensRepository>();
builder.Services.AddScoped<ISuspensionsRepository, SuspensionsRepository>();
builder.Services.AddScoped<IAuditLogsRepository, AuditLogsRepository>();
builder.Services.AddScoped<IBlockedCredentialsRepository, BlockedCredentialsRepository>();
builder.Services.AddScoped<IDeleteRequestsRepository, DeleteRequestsRepository>();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.WebHost.UseUrls("http://0.0.0.0:80");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("Development");
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("Production");
}

app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseMiddleware<AuthorizeHeaderMiddleware>();

app.UseRateLimiter();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();