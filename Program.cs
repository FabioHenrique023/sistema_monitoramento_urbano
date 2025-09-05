using InfraEstrutura;
using sistema_monitoramento_urbano.Models.Repositorio.Entidades;
using sistema_monitoramento_urbano.Models.Repositorio;
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
builder.Services.AddScoped<IRepositorio<Camera>, CameraRepositorio>();
builder.Services.AddScoped<IRepositorio<Video>,  VideoRepositorio>();

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