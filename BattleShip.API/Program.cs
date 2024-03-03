using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// Config
var myIssuer = "http://mysite.com";
var myAudience = "http://myaudience.com";
var mySecret = "asdv234234^&%&^%&^hjsdfb2%%%123456789";
var mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(mySecret));

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<WarService>();
builder.Services.AddSingleton<JwtService>();
builder.Services.AddDbContext<AuthContext>();
builder.Services.AddControllers();
builder.Services.AddGrpc();

builder.Services.AddCors();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, config =>
{
    config.RequireHttpsMetadata = false;
    config.Authority = myIssuer;
    config.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.HttpContext.Request.Headers["Authorization"];
            if (!token.IsNullOrEmpty() && token.ToString().Length > 7)
            {
                token = token.First()!.Substring(7);
            }
            context.Token = token;
            return Task.CompletedTask;
        },
    };
    config.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateLifetime = true,
        ValidateIssuer = true,
        ValidIssuer = myIssuer,
        ValidateAudience = true,
        ValidAudience = myAudience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = mySecurityKey,
    };
});

var app = builder.Build();

// Setup DataBase
using (var scope = app.Services.CreateScope())
{
    var authContext = scope.ServiceProvider.GetRequiredService<AuthContext>();
    if (authContext.Database.EnsureCreated())
    {
        authContext.Commanders.Add(new BattleShip.Models.Commander { Username = "JRM", Password = "HASHED_PASSWORD", Score = 563 });
        authContext.Commanders.Add(new BattleShip.Models.Commander { Username = "You", Password = "HASHED_PASSWORD", Score = 3 });
        authContext.Commanders.Add(new BattleShip.Models.Commander { Username = "_Admin_", Password = "HASHED_PASSWORD", Score = 8 });
        authContext.Commanders.Add(new BattleShip.Models.Commander { Username = "Player1", Password = "HASHED_PASSWORD", Score = 53 });
        authContext.Commanders.Add(new BattleShip.Models.Commander { Username = "Pilot John", Password = "HASHED_PASSWORD", Score = -20 });

        authContext.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(c =>
{
    c.AllowAnyHeader();
    c.AllowAnyMethod();
    c.AllowAnyOrigin();
});
app.UseGrpcWeb();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGrpcService<LeaderboardService>().EnableGrpcWeb();

app.Run();

