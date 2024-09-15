using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Repositories.Contracts;
using ServerLibrary.Repositories.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    {
        // options.UseSqlServer(builder.Configuration.GetConnectionString("AppConStr") ??
        //                      throw new InvalidOperationException("Could not find connection string"))

        var conStrBuilder =
            new SqlConnectionStringBuilder(builder.Configuration.GetConnectionString("DefaultConnection") ??
                                           throw new InvalidOperationException("Could not find connection string"))
                {
                    UserID = builder.Configuration["DbCon:user"],
                    Password = builder.Configuration["DbCon:password"]
                };
        options.UseSqlServer(conStrBuilder.ConnectionString);
    }
);

builder.Services.Configure<JwtSection>(builder.Configuration.GetSection("JwtSection"));
builder.Services.AddScoped<IUserAccount, UserAccountRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();