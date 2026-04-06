using System.Reflection;
using System.Text;
using FFMpegCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Azure.SignalR;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WebsupplyConnect.Application;
using WebsupplyConnect.Application.Configuration;
using WebsupplyConnect.Infrastructure;
using WebsupplyConnect.Infrastructure.ExternalServices.SignalR;

var builder = WebApplication.CreateBuilder(args);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173",
            "https://localhost:5173",
            "http://websupplyconnect.com",
            "https://websupplyconnect.com",
            "http://*.websupplyconnect.com",
            "https://*.websupplyconnect.com"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
        .SetIsOriginAllowedToAllowWildcardSubdomains(); 
    });
});

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "WebsupplyConnect API", 
        Version = "v1",
        Description = "API's para Front-End e App",
        Contact = new OpenApiContact
        {
            Name = "WebsupplyConnect Team",
            Email = "suporte@websupplyconnect.com"
        }
    });

    // Adiciona suporte a comentários XML
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Adiciona suporte a JWT
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        Description = "Insira o JWT token no campo abaixo usando o esquema: Bearer {seu token}",

        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });

    // Configurações adicionais do Swagger
    c.UseAllOfToExtendReferenceSchemas();
    c.UseAllOfForInheritance();
    c.UseOneOfForPolymorphism();
    c.SelectDiscriminatorNameUsing(type => type.Name);
});

builder.Services.Configure<WebhookMetaConfig>(
    builder.Configuration.GetSection("WhatsApp"));

builder.Services.Configure<AzureBlobStorageConfig>(
    builder.Configuration.GetSection("AzureBlobStorageConnection"));

builder.Services.Configure<AzureBusConfig>(
builder.Configuration.GetSection("AzureBusConnection"));

builder.Services.Configure<RedisConfiguration>(
builder.Configuration.GetSection("RedisConnection"));

builder.Services.Configure<AzureAdOptions>(builder.Configuration.GetSection("AzureAd"));


var basePath = AppDomain.CurrentDomain.BaseDirectory;

var ffmpegBinaryPath = Path.Combine(basePath, "Tools"); // Pasta onde estão os binários do FFmpeg
var ffmpegTempPath = Path.Combine(basePath, "Tools", "Temp");// Pasta separada para arquivos temporário

Directory.CreateDirectory(ffmpegTempPath);

// Configura FFmpegCore
GlobalFFOptions.Configure(options =>
{
    options.BinaryFolder = ffmpegBinaryPath;
    options.TemporaryFilesFolder = ffmpegTempPath;
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)
            )
        };

        // Aqui tratamos erros de autorização
        options.Events = new JwtBearerEvents
        {
            OnForbidden = async context =>
            {
                var httpContext = context.HttpContext;

                var mensagem = httpContext.Items["AuthErrorMessage"]?.ToString()
                               ?? "Usuário fora de expediente";
                var showModal = httpContext.Items["ShowModal"] is bool b && b;
                var limite = httpContext.Items["Limite"]?.ToString();

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    message = mensagem,
                    showModal,
                    limite
                });
            }
        };
    });

builder.Services.Configure<MailSenderOptions>(builder.Configuration.GetSection("SendGrid"));

builder.Services.Configure<WebsupplyConnect.Application.Configuration.ETLConfig>(
    builder.Configuration.GetSection("ETL"));
builder.Services.Configure<WebsupplyConnect.Application.Configuration.OLAPConfig>(
    builder.Configuration.GetSection("OLAP"));
builder.Services.Configure<ConversaClassificacaoConfig>(
    builder.Configuration.GetSection("ConversaClassificacao"));

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseCors("AllowSpecificOrigins");

app.UseSwagger();
app.UseSwaggerUI();

app.MapHub<NotificacaoHub>("/notificationHub");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
