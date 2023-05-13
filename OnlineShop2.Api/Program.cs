using Microsoft.EntityFrameworkCore;
using OnlineShop2.Database.Models;
using OnlineShop2.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Cryptography;
using OnlineShop2.Api.Extensions;
using OnlineShop2.Api.Services;
using OnlineShop2.Api.Services.Legacy;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Microsoft.AspNetCore.HttpOverrides;
using System.Diagnostics;
using OnlineShop2.Api.Services.HostedService;
using OnlineShop2.Api.Extensions.MapperProfiles;
using System.Text.Json.Serialization;

namespace OnlineShop2.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            bool isService = !(Debugger.IsAttached || args.Contains("--console"));

            var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

            var builder = WebApplication.CreateBuilder(args);

            if (isService && OperatingSystem.IsWindows())
                builder.Host.UseWindowsService();
            if (isService && OperatingSystem.IsWindows())
                builder.Host.UseSystemd();

                // Add services to the container.
                builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                                  policy =>
                                  {
                                      policy.WithOrigins("http://172.172.172.46:3000").AllowCredentials().AllowAnyHeader().AllowAnyMethod();
                                  });
            });

            builder.Services.AddControllers().AddJsonOptions(optopns=>
                optopns.JsonSerializerOptions.Converters.Add(new JsonDateTimeConverter())
                );
            
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<OnlineShopContext>(option=>
                option.UseNpgsql(builder.Configuration.GetConnectionString("db"))
            );

            builder.Services.AddAutoMapper(typeof(MapperProfileDBModelToDto), typeof(MapperProfileLegacy));

            builder.Services.AddServicesLegacy();

            builder.Services.AddTransient<AuthService>();
            builder.Services.AddTransient<ShopService>();
            builder.Services.AddTransient<InventoryLegacyService>();
            builder.Services.AddTransient<GoodService>();
            builder.Services.AddTransient<GoodGroupService>();
            builder.Services.AddTransient<CurrentBalanceService>();

            builder.Services.AddTransient<SupplierService>();
            builder.Services.AddTransient<ArrivalService>();

            //builder.Services.AddHostedService<ShiftSynchBackgroundService>();
            builder.Services.AddHostedService<SynchLegacyHostedService>();
            builder.Services.AddHostedService<ControlBuyFromInventoryBackgroundService>();

            builder.Services.AddAuthorization();
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        // указывает, будет ли валидироваться издатель при валидации токена
                        ValidateIssuer = true,
                        // строка, представляющая издателя
                        ValidIssuer = AuthOptions.ISSUER,
                        // будет ли валидироваться потребитель токена
                        ValidateAudience = true,
                        // установка потребителя токена
                        ValidAudience = AuthOptions.AUDIENCE,
                        // будет ли валидироваться время существования
                        ValidateLifetime = true,
                        // установка ключа безопасности
                        IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                        // валидация ключа безопасности
                        ValidateIssuerSigningKey = true,
                    };
                });
            /*
            builder.Services.AddSession(options =>
            {
                options.Cookie.Name = ".AdventureWorks.Session";
                options.IdleTimeout = TimeSpan.FromHours(10);
                options.Cookie.IsEssential = true;
            });
            */
            var app = builder.Build();
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseExceptionHandler(handleExcpetionApp =>
            {
                handleExcpetionApp.Run(async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/json";

                    var exceptionHandlerPathFeature =
                        context.Features.Get<IExceptionHandlerPathFeature>();
                    if (exceptionHandlerPathFeature?.Error is MyServiceException)
                        await context.Response.WriteAsync(JsonSerializer.Serialize(new
                        {
                            type = "ServiceError",
                            message = exceptionHandlerPathFeature?.Error.Message,
                            stackTrace = exceptionHandlerPathFeature?.Error.StackTrace
                        }));
                    else
                        await context.Response.WriteAsync(JsonSerializer.Serialize(new
                        {
                            type = "SystemError",
                            message = exceptionHandlerPathFeature?.Error.Message,
                            stackTrace = exceptionHandlerPathFeature?.Error.StackTrace
                        }));
                });
            });
            if(app.Environment.IsDevelopment())
                app.UseHttpsRedirection();
            app.UseCors(MyAllowSpecificOrigins);

            app.UseAuthentication();
            app.UseAuthorization();

            //app.UseSession();

            app.MapControllers();

            using(var scope = app.Services.CreateScope())
                using(var context = scope.ServiceProvider.GetRequiredService<OnlineShopContext>())
            {
                context.Database.Migrate();

                if (!context.Users.Any())
                    context.Users.Add(new User { Login = "xasya", UserName="Alexandr", Password = "kt38hmapq".CreateMD5(), Role = UserRole.Admin });
                if(!context.Shops.Any())
                {
                    context.Shops.Add(new Shop { Alias = "Северный", Adress = "Северный", Inn = "312", OrgName = "Перемышлев" });
                    context.Shops.Add(new Shop { Alias = "Степная", Adress = "Степная", Inn = "312", OrgName = "Перемышлев" });
                    context.Shops.Add(new Shop { Alias = "Разумное", Adress = "Разумное", Inn = "312", OrgName = "Перемышлев" });
                }

                context.SaveChanges();
            };

            app.Run();
        }

        public class AuthOptions
        {
            public const string ISSUER = "OnlineShop2"; // издатель токена
            public const string AUDIENCE = "OnlineShop2Client"; // потребитель токена
            const string KEY = "$parkI1010_ABC_444_dfdfdf!#$";   // ключ для шифрации
            public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
        }

        public class JsonDateTimeConverter : JsonConverter<DateTime>
        {
            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return DateTime.Parse(reader.GetString() ?? DateTime.Now.ToString("dd.MM.yyyy"));
            }

            public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString("dd.MM.yy"));
            }
        }
    }
}