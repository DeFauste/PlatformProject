using CommandService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

//Project
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("InMen"));
builder.Services.AddScoped<ICommandRepo, CommandRepo>();
//

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseDeveloperExceptionPage();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    _ = endpoints.MapControllers(); // подключаем маршрутизацию на контроллеры
});

app.UseHttpsRedirection();

app.Run();

