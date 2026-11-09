using RaceDay.Mvc.Services;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o => { o.IdleTimeout = TimeSpan.FromMinutes(30); o.Cookie.Name = ".RaceDay.Mvc"; o.Cookie.HttpOnly = true; o.Cookie.IsEssential = true; o.Cookie.SameSite = SameSiteMode.Lax; });
builder.Services.AddHttpClient<RaceDayApiClient>(c => { c.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7001/"); c.Timeout = TimeSpan.FromSeconds(60); }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseCookies = false });
var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Error"); app.UseHsts(); }
app.UseHttpsRedirection(); app.UseStaticFiles(); app.UseRouting(); app.UseSession(); app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}"); app.Run();
