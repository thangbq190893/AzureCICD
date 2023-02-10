using Webhook.Repository;
using Webhook.Interface;
using Webhook.Service;
using Webhook.Engine;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "10.1.6.11";
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<SocialManagementRepository>();
builder.Services.AddScoped<ISocialManagementRepository, SocialManagementRepository>();

builder.Services.AddTransient<CacheService>();
builder.Services.AddScoped<ICacheService, CacheService>();

builder.Services.AddTransient<KafkaService>();
builder.Services.AddScoped<IKafkaService, KafkaService>();

builder.Services.AddTransient<FacebookProducer>();
builder.Services.AddTransient<ZaloProducer>();
var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
