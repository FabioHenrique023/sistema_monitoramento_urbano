using InfraEstrutura;
using sistema_monitoramento_urbano.Models.Repositorio.Entidades;
using sistema_monitoramento_urbano.Models.Repositorio;
using sistema_monitoramento_urbano.Models.Services;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Connection Factory (PostgreSQL)
builder.Services.AddSingleton<ISqlConnectionFactory>(sp =>
{
    var cs = sp.GetRequiredService<IConfiguration>()
               .GetConnectionString("Connection");
    if (string.IsNullOrWhiteSpace(cs))
        throw new InvalidOperationException("Connection string 'Connection' não encontrada.");
    return new NpgsqlConnectionFactory(cs);
});

// Repositórios
builder.Services.AddScoped<ICameraRepositorio, CameraRepositorio>();
builder.Services.AddScoped<IVideoRepositorio, VideoRepositorio>();
builder.Services.AddSingleton<GoogleDriveClient>();
builder.Services.Configure<CloudinaryOptions>(builder.Configuration.GetSection("Cloudinary"));
builder.Services.AddSingleton<ICloudinaryService, CloudinaryService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Monitoramento}/{action=Index}/{id?}");

app.Run();