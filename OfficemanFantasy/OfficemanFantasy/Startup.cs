using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;                                                                                                                                 
using Microsoft.Extensions.Hosting;
using OfficemanFantasy.Domain;
using OfficemanFantasy.Domain.Repositories.Abstract;
using OfficemanFantasy.Domain.Repositories.EntityFramework;
using OfficemanFantasy.Service;

namespace OfficemanFantasy
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            //���������� ������ �� appsettings.json
            Configuration.Bind("Project", new Config());

            //���������� ������ ���������� ���������� � �������� ��������
            services.AddTransient<ITextFieldsRepository, EFTextFieldsRepository>();
            services.AddTransient<INpcRepository, EFNpcRepository>();
            services.AddTransient<IUserRepository, EFUserRepository>();
            services.AddTransient<IUnitRepository, EFUnitRepository>();
            services.AddTransient<ITileRepository, EFTileRepository>();
            services.AddTransient<DataManager>();

            //���������� �������� ��
            services.AddDbContext<AppDbContext>(x => x.UseSqlServer(Config.ConnectionString));

            //����������� identity �������
            services.AddIdentity<IdentityUser, IdentityRole>(opts =>
            {
                opts.User.RequireUniqueEmail = true;
                opts.Password.RequiredLength = 6;
                opts.Password.RequireNonAlphanumeric = false;
                opts.Password.RequireLowercase = false;
                opts.Password.RequireUppercase = false;
                opts.Password.RequireDigit = false;
            }).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

            //����������� authentication cookie
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "myCompanyAuth";
                options.Cookie.HttpOnly = true;
                options.LoginPath = "/account/login";
                options.AccessDeniedPath = "/account/accessdenied";
                options.SlidingExpiration = true;
            });

            //����������� �������� ����������� ��� ����� ����*********************************
            services.AddAuthorization(x =>
            {
                x.AddPolicy("AdminArea", policy => { policy.RequireRole("admin"); });
            });

            //��������� ��������� ������������ � ������������� (MVC)
            services.AddControllersWithViews(x =>
            {
                //��� ������� ����� �������� ������ , ������� ��������� ���������
                x.Conventions.Add(new AdminAreaAutorization("Admin", "AdminArea"));
            })
                //��������� ������������� � asp.net core 3.0
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0).AddSessionStateTempDataProvider();


        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            //���������� �� ������� 
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            //���������� ������� �������������
            app.UseRouting();

            //���������� �������������� � �����������
            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseAuthorization();

            //��������� ��������� ����������� ������(css, js � �.�.)
            app.UseStaticFiles();

            //�������� ������ ��� �������� (���������)
            app.UseEndpoints(endpoints =>
            {
               
                endpoints.MapControllerRoute("admin", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
