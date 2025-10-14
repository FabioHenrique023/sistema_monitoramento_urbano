using InfraEstrutura;
using sistema_monitoramento_urbano.Models.Repositorio.Entidades;
using sistema_monitoramento_urbano.Models.Repositorio;
using sistema_monitoramento_urbano.Models.Services;
using Azure.Storage.Blobs;
using Azure.Messaging.ServiceBus;
var builder = WebApplication.CreateBuilder(args);
var cfg = builder.Configuration;

builder.Services.AddSingleton(sp =>
{
    var conn = cfg["Azure:Storage:ConnectionString"]
               ?? Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
    return new BlobServiceClient(conn);
});
builder.Services.AddSingleton(sp =>
{
    var conn = cfg["Azure:ServiceBus:ConnectionString"]
               ?? Environment.GetEnvironmentVariable("AZURE_SERVICEBUS_CONNECTION_STRING");
    return new ServiceBusClient(conn);
});
builder.Services.AddSingleton(sp =>
{
    var conn = cfg["Azure:ServiceBus:ConnectionString"]
               ?? Environment.GetEnvironmentVariable("AZURE_SERVICEBUS_CONNECTION_STRING");
    return new ServiceBusClient(conn);
});

builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<ISqlConnectionFactory>(sp =>
{
    var cs = sp.GetRequiredService<IConfiguration>()
               .GetConnectionString("Connection");
    if (string.IsNullOrWhiteSpace(cs))
        throw new InvalidOperationException("Connection string 'Connection' não encontrada.");
    return new NpgsqlConnectionFactory(cs);
});

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