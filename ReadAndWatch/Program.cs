using Microsoft.EntityFrameworkCore;
using ReadAndWatch.Data;
using ReadAndWatch.Services;


var builder = WebApplication.CreateBuilder(args);

// ✳️ Service eklemeleri burada olmalı
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddSession(); // ✅ BURADA OLMALI
builder.Services.AddHttpContextAccessor(); // ✅ bunu ekle


builder.Services.AddHttpClient<RecommendationService>();


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/User/Login";
        options.LogoutPath = "/User/Logout";
    });

builder.Services.AddAuthorization();



var app = builder.Build();

// ✳️ Middleware'ler burada
app.UseStaticFiles();
app.UseRouting();
app.UseSession(); // ✅ Doğru yer
app.UseAuthentication(); // => Session'dan önce olmalı


app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
