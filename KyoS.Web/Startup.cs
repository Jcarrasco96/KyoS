using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Globalization;

namespace KyoS.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            //passwords details
            services.AddIdentity<UserEntity, IdentityRole>(cfg =>
            {
                cfg.User.RequireUniqueEmail = true;

                cfg.Password.RequireDigit = false;
                cfg.Password.RequiredUniqueChars = 0;
                cfg.Password.RequireLowercase = false;
                cfg.Password.RequireNonAlphanumeric = false;
                cfg.Password.RequireUppercase = false;
                cfg.Password.RequiredLength = 6;

                cfg.Lockout.MaxFailedAccessAttempts = 5;

            })
            .AddEntityFrameworkStores<DataContext>()
            .AddDefaultTokenProviders();

            //not authorized page
            services.ConfigureApplicationCookie(options => 
            {
                //options.LoginPath = "/Account/NotAuthorized";
                options.AccessDeniedPath = "/Account/NotAuthorized";
            });

            //db connection
            services.AddDbContext<DataContext>(cfg =>
            {
                cfg.UseSqlServer(Configuration.GetConnectionString("KyoSConnection"));
            });

            //session variables
            services.AddSession(options =>
            {
                //options.Cookie.Name = ".AdventureWorks.Session";
                options.IdleTimeout = System.TimeSpan.FromSeconds(10);
                options.Cookie.IsEssential = true;
            });

            services.AddTransient<SeedDb>();
            services.AddScoped<IImageHelper, ImageHelper>();
            services.AddScoped<IConverterHelper, ConverterHelper>();
            services.AddScoped<IUserHelper, UserHelper>();
            services.AddScoped<ICombosHelper, CombosHelper>();
            services.AddScoped<IClassificationHelper, ClassificationHelper>();
            services.AddScoped<IRenderHelper, RenderHelper>();
            services.AddScoped<IDateHelper, DateHelper>();
            services.AddScoped<ITranslateHelper, TranslateHelper>();
            services.AddScoped<IReportHelper, ReportHelper>();
            services.AddScoped<IMimeType, MimeType>();
            
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddRazorPages();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/error/{0}");
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseFastReport();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });

            //system culture
            var cultureInfo = new CultureInfo("en-US");
            cultureInfo.DateTimeFormat.DateSeparator = "/";
            cultureInfo.DateTimeFormat.ShortDatePattern = "MM/dd/yyyy";
            cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
            cultureInfo.NumberFormat.CurrencyDecimalSeparator = ".";
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
        }
    }
}
