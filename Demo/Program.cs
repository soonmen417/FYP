global using Demo;
global using Demo.Models;
using DNTCaptcha.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSqlServer<DB>($@"
    Data Source=(LocalDB)\MSSQLLocalDB;
    AttachDbFilename={builder.Environment.ContentRootPath}\DB.mdf;
");
builder.Services.AddAuthentication().AddCookie();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Helper>();
builder.Services.AddWebOptimizer(pipeline =>
{
    pipeline.AddCssBundle("/bundle.css",
        "/css/pager.css",
        "/css/app.css");
    pipeline.AddJavaScriptBundle("/bundle.js",
        "/js/jquery.min.js",
        "/js/jquery.unobtrusive-ajax.min.js",
        "/js/jquery.validate.min.js",
        "/js/jquery.validate.unobtrusive.min.js",
        "/js/app.js");
});
// TODO: Add SignalR
builder.Services.AddSignalR(options => {
    options.MaximumReceiveMessageSize = null; // Default 32KB
});
//Captcha
builder.Services.AddDNTCaptcha(options =>
{

    options.UseCookieStorageProvider(SameSiteMode.Strict)

    .AbsoluteExpiration(minutes: 7)
    .ShowThousandsSeparators(false)
    .WithNoise(pixelsDensity: 25, linesCount: 3)
    .WithEncryptionKey("This is my secure key!")
    .InputNames(
        new DNTCaptchaComponent
        {
            CaptchaHiddenInputName = "DNTCaptchaText",
            CaptchaHiddenTokenName = "DNTCaptchaToken",
            CaptchaInputName = "DNTCaptchaInputText"
        })
    .Identifier("dntCaptcha")
    ;
}); ;

var app = builder.Build();
app.UseFileServer();
app.UseHttpsRedirection();
app.UseWebOptimizer();
app.UseStaticFiles();
app.UseSession();
app.UseRequestLocalization(options => options.SetDefaultCulture("en-MY"));
// Map SignalR hub --> "/hub"
app.MapHub<ChatHub>("/hub");
app.MapDefaultControllerRoute();
app.Run();