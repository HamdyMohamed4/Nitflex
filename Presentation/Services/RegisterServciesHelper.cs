using ApplicationLayer.Contract;
using ApplicationLayer.Mapping;
using ApplicationLayer.Services;
using ApplicationLayer.Services.Payment;
using InfrastructureLayer;
using InfrastructureLayer.Contracts;
using InfrastructureLayer.Repositories;
using InfrastructureLayer.UserModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.SemanticKernel;
using Presentation.Services;
using System.Text;

namespace Presentation.Services
{
    public class RegisterServicesHelper
    {
        public static void RegisteredServices(WebApplicationBuilder builder)
        {

            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' followed by space and your token."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
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


            builder.Services.AddScoped<IProfileService, ProfileService>();
            builder.Services.AddScoped<IFileService, FileService>();


            // DbContext
            builder.Services.AddDbContext<NetflixContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<NetflixContext>()
            .AddDefaultTokenProviders();

            // JWT
            var jwtSecretKey = builder.Configuration.GetValue<string>("JwtSettings:SecretKey");
            var key = Encoding.ASCII.GetBytes(jwtSecretKey);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });

            // AutoMapper
            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            // Repositories & Services
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped(typeof(IViewRepository<>), typeof(ViewRepository<>));
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IPaymentMethods, PaymentMethodsService>();


            builder.Services.AddScoped<IGenreService, GenreService>();
            builder.Services.AddScoped<IMovieService, MovieService>();
            builder.Services.AddScoped<IRatingService, RatingService>();
            builder.Services.AddScoped<ISubsciptionService, SubcriptionService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IOtpRepository, OtpRepository>();


            builder.Services.AddHttpClient<PayPalGateway>();
            builder.Services.AddHttpClient<PaymobGateway>();
            builder.Services.AddScoped<PaymentFactory>();

            builder.Services.AddSingleton<TokenService>();
            builder.Services.AddScoped<IRefreshTokens, RefreshTokenService>();
            builder.Services.AddScoped<IRefreshTokenRetriver, RefreshTokenRetriverService>();

            builder.Services.AddSingleton<EmailService>();

            // Register semantic kernel services
            builder.Services.AddSingleton<IChatHistoryService, ChatHistoryService>();
            builder.Services.AddSingleton<IChatService, ChatService>();

            builder.Services.AddHttpClient("gemini", client =>
            {
                client.BaseAddress = new Uri("https://generativelanguage.googleapis.com/v1beta/");
                client.DefaultRequestHeaders.Add("x-goog-api-key", builder.Configuration["GeminiApiKey"]);
            });

            builder.Services.AddSingleton<Kernel>(options =>
            {
                var kernelBuilder = Kernel.CreateBuilder();

                var httpClient = options.GetRequiredService<IHttpClientFactory>().CreateClient("gemini");

                kernelBuilder.AddOpenAIChatCompletion(
                    modelId: "models/gemini-2.0-flash",
                    serviceId: "gemini-pro",
                    apiKey: builder.Configuration["GeminiApiKey"]!,
                    httpClient: httpClient
                );

                kernelBuilder.Services.AddLogging(loggingBuilder =>
                {
                    loggingBuilder.AddConsole();
                    loggingBuilder.SetMinimumLevel(LogLevel.Information);
                });

                return kernelBuilder.Build();
            });
        }
    }
}
