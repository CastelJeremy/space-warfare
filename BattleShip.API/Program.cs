using BattleShip.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

// using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

// Config
var myIssuer = "http://mysite.com";
var myAudience = "http://myaudience.com";
var mySecret = "asdv234234^&%&^%&^hjsdfb2%%%123456789";
var mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(mySecret));

// Setup Database
await using var ctx = new Context();
// await ctx.Database.EnsureDeletedAsync();
await ctx.Database.EnsureCreatedAsync();
await ctx.SaveChangesAsync();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<WarService>();
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
            if (!token.IsNullOrEmpty())
            {
                token = token.First()!.Substring(7);
            }
            context.Token = token;
            Console.WriteLine();
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

// var mySecret = "asdv234234^&%&^%&^hjsdfb2%%%123456789";
// var mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(mySecret));
// 
// var myIssuer = "http://mysite.com";
// var myAudience = "http://myaudience.com";
// 
// var tokenHandler = new JwtSecurityTokenHandler();
// try
// {
//     tokenHandler.ValidateToken(token, new TokenValidationParameters
//     {
//         ValidateIssuerSigningKey = true,
//         ValidateIssuer = true,
//         ValidateAudience = true,
//         ValidIssuer = myIssuer,
//         ValidAudience = myAudience,
//         IssuerSigningKey = mySecurityKey
//     }, out SecurityToken validatedToken);
// }
// catch
// {
//     return false;
// }
// return true;

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.UseCors(c =>
{
    c.AllowAnyHeader();
    c.AllowAnyMethod();
    c.AllowAnyOrigin();
});

app.MapGet("/war/{id}", Results<NotFound, JsonHttpResult<WarDto>> (WarService warService, [FromRoute] Guid id) =>
{
    if (!warService.Wars.ContainsKey(id))
    {
        return TypedResults.NotFound();
    }

    War war = warService.Wars[id];

    return TypedResults.Json(war.ToDto());
})
.WithName("GetWar")
.WithOpenApi();

app.MapPost("/war/beam/{id}", Results<NotFound, JsonHttpResult<BeamResponseDto>> (WarService warService, [FromRoute] Guid id, [FromBody] BeamActionDto beamAction) =>
{
    if (!warService.Wars.ContainsKey(id))
    {
        return TypedResults.NotFound();
    }

    War war = warService.Wars[id];

    return TypedResults.Json(war.Beam(beamAction));
}).WithName("Beam")
.WithOpenApi();

app.MapPost("/war", (WarService warService) =>
{
    War war = new War();
    warService.Wars.Add(war.Id, war);
    // return game.PlayerGrid.ToJaggedArray();
    return TypedResults.Json(war.ToDto());
})
.WithName("CreateWar")
.WithOpenApi();

app.MapPost("/auth/register", Results<BadRequest, JsonHttpResult<TokenDto>> (HttpRequest request, [FromBody] CommanderRegisterDto commanderRegister) =>
{
    var query = ctx.Commanders.Where(c => c.Username.Equals(commanderRegister.Username));
    if (!query.IsNullOrEmpty())
    {
        return TypedResults.BadRequest();
    }

    SHA3_256 sha256 = SHA3_256.Create();
    string hashedPassword = Convert.ToHexString(sha256.ComputeHash(Encoding.UTF8.GetBytes(commanderRegister.Password)));

    Commander commander = new Commander { Username = commanderRegister.Username, Password = hashedPassword };
    ctx.Commanders.Add(commander);
    ctx.SaveChanges();

    var tokenHandler = new JwtSecurityTokenHandler();
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new Claim[]
        {
            new Claim("id", commander.Id.ToString()),
        }),
        Expires = DateTime.UtcNow.AddDays(3),
        Issuer = myIssuer,
        Audience = myAudience,
        SigningCredentials = new SigningCredentials(mySecurityKey, SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);

    Session session = new Session
    {
        CommanderId = commander.Id,
        CreationDate = DateTime.UtcNow,
        Ip = request.HttpContext.Connection.RemoteIpAddress!.ToString(),
        Fingerprint = request.Headers["User-Agent"]!
    };

    ctx.Sessions.Add(session);
    ctx.SaveChanges();

    return TypedResults.Json(new TokenDto { AccessToken = tokenHandler.WriteToken(token) });
})
.WithName("CreateCommander")
.WithOpenApi();

app.MapPost("/auth/login", Results<BadRequest, JsonHttpResult<TokenDto>> (HttpRequest request, [FromBody] CommanderLoginDto commanderLogin) =>
{
    var query = ctx.Commanders.Where(c => c.Username.Equals(commanderLogin.Username));
    if (query.IsNullOrEmpty())
    {
        return TypedResults.BadRequest();
    }

    Commander commander = query.First();
    SHA3_256 sha256 = SHA3_256.Create();
    string hashedPassword = Convert.ToHexString(sha256.ComputeHash(Encoding.UTF8.GetBytes(commanderLogin.Password)));
    if (hashedPassword != commander.Password)
    {
        return TypedResults.BadRequest();
    }

    var tokenHandler = new JwtSecurityTokenHandler();
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new Claim[]
        {
            new Claim("id", commander.Id.ToString()),
        }),
        Expires = DateTime.UtcNow.AddDays(3),
        Issuer = myIssuer,
        Audience = myAudience,
        SigningCredentials = new SigningCredentials(mySecurityKey, SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);

    Session session = new Session
    {
        CommanderId = commander.Id,
        CreationDate = DateTime.UtcNow,
        Ip = request.HttpContext.Connection.RemoteIpAddress!.ToString(),
        Fingerprint = request.Headers["User-Agent"]!
    };

    ctx.Sessions.Add(session);
    ctx.SaveChanges();

    return TypedResults.Json(new TokenDto { AccessToken = tokenHandler.WriteToken(token) });
})
.RequireAuthorization()
.WithName("LoginCommander")
.WithOpenApi();

app.MapGet("/sessions", Results<UnauthorizedHttpResult, JsonHttpResult<SessionDto[]>> (HttpContext context) =>
{
    ClaimsIdentity? identity = context.User.Identity as ClaimsIdentity;
    Guid? commanderId = new Guid(identity!.FindFirst("id")!.Value);
    if (identity is null || commanderId is null)
    {
        return TypedResults.Unauthorized();
    }

    SessionDto[] sessions = [];
    var query = ctx.Sessions.Where(s => s.CommanderId.Equals(commanderId));
    if (!query.IsNullOrEmpty())
    {
        sessions = query.Select((s) => new SessionDto
        {
            Id = s.Id,
            CreationDate = s.CreationDate,
            Ip = s.Ip,
            Fingerprint = s.Fingerprint
        }).ToArray();
    }

    return TypedResults.Json(sessions);
})
.RequireAuthorization()
.WithName("GetSessions")
.WithOpenApi();

app.MapDelete("/sessions/{sessionId}", Results<UnauthorizedHttpResult, NotFound, Ok> (HttpContext context, [FromRoute] Guid sessionId) =>
{
    ClaimsIdentity? identity = context.User.Identity as ClaimsIdentity;
    Guid? commanderId = new Guid(identity!.FindFirst("id")!.Value);
    if (identity is null || commanderId is null)
    {
        return TypedResults.Unauthorized();
    }

    var query = ctx.Sessions.Where(s => s.Id.Equals(sessionId) && s.CommanderId.Equals(commanderId));
    if (query.IsNullOrEmpty())
    {
        return TypedResults.NotFound();
    }

    Session session = query.First();
    ctx.Sessions.Remove(session);
    ctx.SaveChanges();

    return TypedResults.Ok();
})
.RequireAuthorization()
.WithName("DeleteSession")
.WithOpenApi();

app.Run();
/**


var c = new Commander { Username = "USERNAME", Password = "PASSWORD" };
ctx.Commanders.Add(c);
await ctx.SaveChangesAsync();

var s = new Session { CommanderId = c.Id, Ip = "127.0.0.1", Fingerprint = "Firefox" };
ctx.Sessions.Add(s);

await ctx.SaveChangesAsync();

var fCommanders = await ctx.Commanders.Where(c => c.Username.StartsWith("USER")).ToListAsync();
var fSessions = await ctx.Sessions.ToListAsync();

Console.WriteLine(fCommanders.First().Username);
Console.WriteLine(fSessions.First().Commander.Username);

*/

