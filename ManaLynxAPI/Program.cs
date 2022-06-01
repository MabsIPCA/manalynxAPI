using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using ManaLynxAPI.Data;
using ManaLynxAPI.Utils;
using ManaLynxAPI.Controllers;
using Quartz;
using Quartz.Spi;
using Quartz.Impl;
using ManaLynxAPI.Hosting;
using Serilog;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Database Connection
builder.Services.AddControllers().AddJsonOptions(x =>
                {
                    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                }); ;
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection"), o =>
    {
        o.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromMinutes(5),
            errorNumbersToAdd: new List<int> { 4060 });
    }
    ));
builder.Services.AddCors();


#region Serilog

var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);
builder.Services.AddTransient<ILoggerUtils, LoggerUtils>();

#endregion

#region JobDefinitions
builder.Services.AddHostedService<QuartzService>();
builder.Services.AddSingleton<IJobFactory, SingletonJobFactory>();
builder.Services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
builder.Services.AddSingleton<JobReminders>();
builder.Services.AddSingleton(new MyJob(type: typeof(JobReminders), expression: "0 0 12 * * ?"));   //Fire every Day at 12PM
//builder.Services.AddSingleton(new MyJob(type: typeof(JobReminders), expression: "0/30 0/1 * 1/1 * ? *")); //Fire every 30 sec
#endregion

#region Authentication
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
var securityScheme = new OpenApiSecurityScheme()
{
    Name = "Authorization",
    Type = SecuritySchemeType.ApiKey,
    Scheme = "Bearer",
    In = ParameterLocation.Header,
    Description = "JWT auth",
};
var securityRequirements = new OpenApiSecurityRequirement()
{
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer",
            }
        },
        new string[]{}
    }
};
var contactInfo = new OpenApiContact()
{
    Name = "ManaLynx",
    Email = "m",
    Url = new Uri("https://asd.com"),
};
var license = new OpenApiLicense()
{
    Name = "License",
};
var info = new OpenApiInfo()
{
    Version = "V1",
    Title = "ManaLynxAPI",
    Description = "ManaLynx API for service test.",
    Contact = contactInfo,
    License = license,
};

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", info);
    options.AddSecurityDefinition("Bearer", securityScheme);
    options.AddSecurityRequirement(securityRequirements);
});
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
                .AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
    };
});

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddTransient<IJWTAuthManager, JWTAuthManager>();
#endregion

#region Utils
builder.Services.AddTransient<IAppUtils, AppUtils>();
builder.Services.AddTransient<ILoginCredentialUtils, LoginCredentialUtils>();
builder.Services.AddTransient<IPessoaUtils, PessoaUtils>();
builder.Services.AddTransient<IClienteUtils, ClienteUtils>();
builder.Services.AddTransient<IDadoClinicoUtils, DadoClinicoUtils>();
builder.Services.AddTransient<IPagamentoUtils, PagamentoUtils>();
builder.Services.AddTransient<ICoberturaUtils, CoberturaUtils>();
builder.Services.AddTransient<IAgenteUtils, AgenteUtils>();
builder.Services.AddTransient<IApoliceUtils, ApoliceUtils>();
builder.Services.AddTransient<ISinistroUtils, SinistroUtils>();
builder.Services.AddTransient<IManaUserUtils, ManaUserUtils>();
builder.Services.AddTransient<IEquipaUtils, EquipaUtils>();
builder.Services.AddTransient<IGestorUtils, GestorUtils>();
builder.Services.AddTransient<IVeiculoUtils, VeiculoUtils>();
builder.Services.AddTransient<IProvaUtils, ProvaUtils>();
builder.Services.AddTransient<IRelatorioPeritagemUtils, RelatorioPeritagemUtils>();
#endregion

var app = builder.Build();


app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
//app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();