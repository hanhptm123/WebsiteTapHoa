using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using TapHoa.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// C?u hình Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();

// C?u hình Session
builder.Services.AddDistributedMemoryCache(); // B? nh? t?m trong RAM
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Th?i gian h?t h?n session
    options.Cookie.HttpOnly = true; // Ch? có server truy c?p ???c cookie này
    options.Cookie.IsEssential = true; // Cookie là c?n thi?t
});

// C?u hình k?t n?i c? s? d? li?u
builder.Services.AddDbContext<TaphoaContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("TAPHOA"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Kích ho?t Authentication và Session Middleware
app.UseAuthentication();
app.UseAuthorization();
app.UseSession(); // Quan tr?ng: ??t sau Authentication và tr??c ??nh tuy?n

// ??nh tuy?n Controller
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
