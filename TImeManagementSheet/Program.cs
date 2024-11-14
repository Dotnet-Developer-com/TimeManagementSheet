using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

 
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using static Org.BouncyCastle.Math.EC.ECCurve;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args); 
        builder.Services.AddSession();
        builder.Services.AddMvc(options => options.EnableEndpointRouting = false);
        builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();

        builder.Services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = null);
        
        var app = builder.Build();
        //app.UseWebMarkupMin();

        //var env = app.HostingEnvironment;

        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        builder.Configuration.AddJsonFile($"appsettings.{builder.Environment}.json", optional: true, reloadOnChange: true);
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }  
        app.UseSession();
        app.UseAuthentication();
        //app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthorization();
        app.UseFileServer();
        //app.Urls.Add("https://103.216.211.104:5077");
        app.MapControllerRoute(
            name: "Admin",
            pattern: "{area:exists}/{controller=Login}/{action=Login}/{id?}");
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Login}/{action=Login}/{id?}");
        
        app.Run();
    }
}