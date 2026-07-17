using AutoMarket.Data;
using AutoMarket.Models;
using AutoPrice.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// ── Base de dados ────────────────────────────────────────────────────────────
// O AutoPrice liga-se à mesma base de dados do AutoMarket. É esta ligação —
// e não uma chamada HTTP a controllers da API — que agora faz a ponte entre
// as duas aplicações: cada uma corre de forma independente, e o que uma
// escreve na BD a outra vê no pedido seguinte.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Identity ──────────────────────────────────────────────────────────────────
// Mesma configuração usada no AutoMarket, para que login/registo validem as
// passwords exatamente da mesma forma nos dois lados (a tabela de utilizadores
// é a mesma, gerida pelo Identity).
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Serviço próprio para emitir o JWT depois de validar as credenciais localmente
// (antes essa emissão vivia só no AuthController do AutoMarket).
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<FotoUploadService>();

// Autenticação com a ferramenta oficial da Microsoft (JwtBearer), em vez de
// descodificarmos o token e construirmos os claims manualmente.
// O middleware valida a assinatura, o issuer, a audience e a validade do token.
// Nota: é preciso registar isto DEPOIS do AddIdentity, porque o AddIdentity também
// mexe no esquema de autenticação por omissão — assim garantimos que o esquema
// ativo para [Authorize] continua a ser o JwtBearer (mesmo padrão do AutoMarket).
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!))
        };

        // O token não vem no cabeçalho Authorization (é um pedido de página, não de API),
        // vem guardado no cookie "token" — dizemos ao middleware para ir buscá-lo ali.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies["token"];
                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.Run();