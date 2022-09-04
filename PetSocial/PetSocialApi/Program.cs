using Mailing;
using PetSocialApi.Services;
using PetSocialDb;
using SharedApi;
using SharedApi.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddPetSocialDb(builder.Configuration);
builder.Services.AddMailing(builder.Configuration);
builder.Services.AddSharedApi(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<IApiAuthService, AuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(options =>
{// TODO: strict
    options
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();