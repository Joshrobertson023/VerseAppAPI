using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Oracle.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using System.Text;
using VerseAppAPI;
using VerseAppAPI.Controllers;
using VerseAppAPI.Models;
using static System.Reflection.Metadata.BlobBuilder;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseOracle(
        builder.Configuration.GetConnectionString("Default"),
        oracleOpts =>
        {
            oracleOpts.CommandTimeout(2);
        }
    ));


builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<ApplicationDBContext>()
    .AddDefaultTokenProviders();

// 2) Configure JWT‐Bearer:
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

// 3) Register your controllers and any scoped services:
builder.Services.AddScoped<UserControllerDB>();
builder.Services.AddScoped<VerseControllerDB>();
builder.Services.AddControllers();

// 4) Configure Swagger (if in Development):
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 5) Configure CORS:
var MyAllowAllOrigins = "_myAllowAllOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowAllOrigins, policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
var config = app.Services.GetRequiredService<IConfiguration>();

app.Lifetime.ApplicationStarted.Register(() =>
{

    Task.Run(async () =>
    {
        try
        {
            using var conn = new OracleConnection(config.GetConnectionString("Default"));
            await conn.OpenAsync();  // ~200 ms the very first time
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT USERNAME FROM USERS WHERE ROWNUM = 1";
            var result = await cmd.ExecuteScalarAsync();  // brings index blocks into cache
            logger.LogInformation("Warmup SQL ran successfully. Result: {UserName}", result);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Warm-up encountered an exception.");
        }
    });
    Task.Run(async () =>
    {
        try
        {
            using var conn = new OracleConnection(config.GetConnectionString("Default"));
            await conn.OpenAsync();  // ~200 ms the very first time
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM USERS WHERE ROWNUM = 1";
            var result = await cmd.ExecuteScalarAsync();  // brings index blocks into cache
            logger.LogInformation("Warmup All SQL ran successfully. Result: {UserName}", result);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Warm-up All encountered an exception.");
        }
    });
    //Task.Run(async () =>
    //{
    //    try
    //    {
    //        string query = @"
    //                        SELECT * FROM VERSES WHERE VERSE_REFERENCE = :reference";

    //        using OracleConnection conn = new OracleConnection(config.GetConnectionString("Default"));
    //        await conn.OpenAsync();

    //        using OracleCommand cmd = new OracleCommand(query, conn);

    //        string reference = "";
    //        int saved = 0;
    //        int highlighted = 0;
    //        string text = "";

    //        var referenceParameter = new OracleParameter("reference", reference);
    //        var savedParameter = new OracleParameter("saved", saved);
    //        var highlightedParameter = new OracleParameter("highlighted", highlighted);
    //        var textParameter = new OracleParameter("text", text);
    //        cmd.Parameters.Add(referenceParameter);
    //        cmd.Parameters.Add(savedParameter);
    //        cmd.Parameters.Add(highlightedParameter);
    //        cmd.Parameters.Add(textParameter);

    //        //for (int i = 10; i < 18; i++)
    //        for (int i = 0; i < 14; i++)
    //        {
    //            for (int j = 14; j <= BibleStructure.GetNumberChapters(VerseControllerDB.books[i]); j++)
    //            {
    //                for (int k = 0; k < BibleStructure.GetNumberVerses(VerseControllerDB.books[i], j); k++)
    //                {
    //                    List<int> verse = new List<int>() { k + 1 };
    //                    referenceParameter.Value = ReferenceParse.ConvertToReferenceString(VerseControllerDB.books[i], j, verse);
    //                    savedParameter.Value = 0;
    //                    highlightedParameter.Value = 0;
    //                    VerseModel verseModel = await BibleAPI.GetAPIVerseAsync(ReferenceParse.ConvertToReferenceString(VerseControllerDB.books[i], j, verse), "kjv");
    //                    textParameter.Value = verseModel.Text;

    //                    await cmd.ExecuteNonQueryAsync();
    //                    logger.LogInformation("Inserted verse: {Reference}", referenceParameter.Value);
    //                    await Task.Delay(2500);
    //                }
    //            }
    //        }

    //        conn.Close();
    //        conn.Dispose();
    //    }
    //    catch (Exception ex)
    //    {
    //        logger.LogWarning(ex, "Error inserting all verses.");
    //    }
    //});

});

// 6) In Development, enable SwaggerUI:
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 7) Redirect HTTP → HTTPS:
app.UseHttpsRedirection();

// 8) Routing must come before CORS/auth:
app.UseRouting();

// 9) Apply CORS (must be between UseRouting and UseAuthentication/UseAuthorization):
app.UseCors(MyAllowAllOrigins);

// 10) Authentication/Authorization:
app.UseAuthentication();
app.UseAuthorization();

// 11) Map controller endpoints:
app.MapControllers();

app.Run();
