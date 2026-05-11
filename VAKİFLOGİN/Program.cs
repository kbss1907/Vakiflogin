using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Antiforgery;
using System.Text;
using VAKIFLOGIN.Data;
using VAKIFLOGIN.Services;

var builder = WebApplication.CreateBuilder(args);

// --- SERVİSLER ---
builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();
builder.Services.AddOpenApi();

builder.Services.AddScoped<DbConnectionFactory>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ILeaveRepository, LeaveRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();

// --- JWT & BFF AUTH ---
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {// 1. ADIM: Tarayıcıdan gelen 'VakifSessionId' çerezini oku
                var sessionId = context.Request.Cookies["VakifSessionId"];
                if (!string.IsNullOrEmpty(sessionId)) // 2. ADIM: Eğer bu ID bellekte (IMemoryCache) varsa, 
            // karşılık gelen gerçek JWT'yi (token) bul ve sisteme 'Token budur' de
                   {
                    var cache = context.HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
                    if(cache.TryGetValue(sessionId, out object? sessionObj))
                    {
                        // dynamic kullanarak kutunun içindekilere kolayca ulaşıyoruz
                        dynamic? sessionData = sessionObj;
                        string? storedIp = sessionData?.IP;
                        string? token = sessionData?.AccessToken;
                        // KRİTİK KONTROL: Şu anki IP, girişteki IP ile aynı mı?
                        var currentIp = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                        if (storedIp == currentIp)
                        {
                            context.Token = token;
                            

                        }
                        else{
                            Console.WriteLine($"[GÜVENLİK İHLALİ] IP Uyuşmazlığı! Beklenen: {storedIp}, Gelen: {currentIp}");
                        }
                        
                        
                    }
                   }
                    
                
                return Task.CompletedTask;
            }
        };
    });

// --- CORS & RATE LIMITER ---
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", policy => 
        policy.WithOrigins("http://localhost:4200").AllowAnyMethod().AllowAnyHeader().AllowCredentials());
});

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("loginPolicy", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 5;
    });
});

// --- KURUMSAL ANTIFORGERY (CSRF) AYARLARI ---
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
    options.Cookie.Name = "VakifAntiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.Path = "/"; // Tüm sayfa ve API yollarında geçerli olsun
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

var app = builder.Build();

// --- ARA KATMAN (MIDDLEWARE) SİLSİLESİ (BEST PRACTICE ✅) ---
app.UseHttpsRedirection();
app.UseRouting(); // Routing her zaman önce gelmeli

app.UseCors("AllowAll");
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// Antiforgery, Authorization'dan sonra ve Endpoint'lerden önce gelmeli
app.UseAntiforgery();

app.Use((context, next) =>
{
    // Sadece tarayıcıda XSRF-TOKEN çerezi yoksa yeni bir tane üret ve gönder.
    // Bu sayede her istekte çerez güncellenmez ve paralel isteklerde '400 Bad Request' hatası alınmaz.
    if (!context.Request.Cookies.ContainsKey("XSRF-TOKEN"))
    {
        var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
        var tokens = antiforgery.GetAndStoreTokens(context);
        context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!, 
            new CookieOptions { 
                HttpOnly = false, 
                SameSite = SameSiteMode.Lax,
                Path = "/" 
            });
    }
    
    return next(context);
});

app.MapControllers();
app.Run();
