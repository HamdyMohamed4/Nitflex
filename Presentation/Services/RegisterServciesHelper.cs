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
using System.Text;
using Presentation.Services;

namespace Presentation.Services
{
    public class RegisterServicesHelper
    {
        public static void RegisteredServices(WebApplicationBuilder builder)
        {
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
            

            builder.Services.AddHttpClient<PayPalGateway>();
            builder.Services.AddHttpClient<PaymobGateway>();
            builder.Services.AddScoped<PaymentFactory>();

            builder.Services.AddSingleton<TokenService>();
            builder.Services.AddScoped<IRefreshTokens, RefreshTokenService>();
            builder.Services.AddScoped<IRefreshTokenRetriver, RefreshTokenRetriverService>();

            builder.Services.AddSingleton<EmailService>();

        }
    }
}
