using LabSolution.Notifications.EmailService;
using LabSolution.Infrastructure;
using LabSolution.Models;
using LabSolution.Notifications;
using LabSolution.Services;
using LabSolution.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;

namespace LabSolution
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            StaticConfig = configuration;
        }

        public IConfiguration Configuration { get; }

        public static IConfiguration StaticConfig { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
            services.AddSingleton(Configuration.GetSection(nameof(EmailConfiguration)).Get<EmailConfiguration>());
            services.Configure<AppEmailNotificationConfig>(options => Configuration.GetSection(nameof(AppEmailNotificationConfig)).Bind(options));

            services.AddControllers();

            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAppConfigService, AppConfigService>();
            services.AddSingleton<IPdfReportProvider, PdfReportProvider>();
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<INotificationManager, NotificationManager>();

            services.AddSingleton(Log.Logger);

            services.AddDbContext<LabSolutionContext>(options => options.UseSqlServer(Configuration.GetConnectionString("Default")));

            services.AddHealthChecks();

            services.AddApplicationInsightsTelemetry();

            services.AddCors(options =>
            {
                options.AddPolicy("EnableCORS", builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                });
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "LabSolution", Version = "v1" });
            });

            services.AddAuthentication(opts =>
            {
                opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options => {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = Configuration["AppSecurityOptions:Issuer"],
                    ValidAudience = Configuration["AppSecurityOptions:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["AppSecurityOptions:TokenKey"]))
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                EnableSwagger(app);
            }
            else
            {
                if (string.Equals(Configuration.GetSection("UseSwaggerInProd").Value, "true", System.StringComparison.InvariantCultureIgnoreCase))
                    EnableSwagger(app);
            }

            app.UseHttpsRedirection();
            app.UseCors("EnableCORS");
            app.UseSerilogRequestLogging();

            app.UseRouting();

            app.UseHealthChecks("/status/health");

            app.UseMiddleware<ExceptionMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static void EnableSwagger(IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmallHrApi v1"));
        }
    }
}
