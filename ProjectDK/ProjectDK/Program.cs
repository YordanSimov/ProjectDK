using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProjectDK.BL.CommandHandlers;
using ProjectDK.DL.Repositories.MsSQL;
using ProjectDK.Extensions;
using ProjectDK.HealthChecks;
using ProjectDK.Middleware;
using ProjectDK.Models.Configurations;
using ProjectDK.Models.Models;
using ProjectDK.Models.Models.Users;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System.Text;

var logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(theme: AnsiConsoleTheme.Code)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddSerilog(logger);

//Kafka
builder.Services.Configure<KafkaProducerSettings>(builder
    .Configuration.GetSection(nameof(KafkaProducerSettings)));

builder.Services.Configure<KafkaConsumerSettings>(builder
    .Configuration.GetSection(nameof(KafkaConsumerSettings)));

//Mongo
builder.Services.Configure<MongoDbSettings>(builder
    .Configuration.GetSection(nameof(MongoDbSettings)));

//HttpClient
builder.Services.Configure<HttpClientSettings>(builder
    .Configuration.GetSection(nameof(HttpClientSettings)));

builder.Services.AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();

builder.Services.AddValidatorsFromAssemblyContaining(typeof(Program));

// Add services to the container.
builder.Services.RegisterRepositories()
    .RegisterServices()
    .RegisterCache<int,Book>()
    .AddAutoMapper(typeof(Program));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(x =>
{
    var jwtSecurityScheme = new OpenApiSecurityScheme()
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "JWT Authentication",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Put **_ONLY_** your JWT Bearer token in the text box below",
        Reference = new OpenApiReference()
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };
    x.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    x.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {jwtSecurityScheme,Array.Empty<string>()}
    });
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("View", policy =>
    {
        policy.RequireClaim("View");
    });
    options.AddPolicy("Test", policy =>
    {
        policy.RequireClaim("Test");
    });
});

//Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey
            (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

//healthchecks
builder.Services.AddHealthChecks()
    .AddCheck<TestHealthCheck>("Test")
    .AddCheck<SQLHealthCheck>("SQL Server")
    .AddUrlGroup(new Uri("https://google.com"), name: "Google Service");

//MediatR
builder.Services.AddMediatR(typeof(GetAllBooksCommandHandler).Assembly);

// Identity
builder.Services.AddIdentity<UserInfo, UserRole>()
    .AddUserStore<UserInfoStore>()
    .AddRoleStore<UserRoleStore>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.UseAuthentication();

app.UseHttpsRedirection();

app.MapControllers();

app.RegisterHealthChecks();

app.UseMiddleware<ErrorHandlerMiddleware>();

app.Run();
