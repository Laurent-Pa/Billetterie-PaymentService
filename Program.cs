using PaymentService.Services.Implementations;
using PaymentService.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Payment Service API",
        Version = "v1",
        Description = "Microservice de gestion des paiements pour Billetterie-Spectacles"
    });
});

// Configuration du service de paiement (Mock ou Stripe)
var useMockPayment = builder.Configuration.GetValue<bool>("PaymentService:UseMockPayment");

if (useMockPayment)
{
    builder.Services.AddSingleton<IPaymentService, MockPaymentService>();
    Console.WriteLine("Using MOCK Payment Service");
}
else
{
    builder.Services.AddScoped<IPaymentService, StripePaymentService>();
    Console.WriteLine("Using STRIPE Payment Service");
}

// CORS pour permettre au monolithe d'appeler ce service
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMonolith", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowMonolith");

app.UseAuthorization();

app.MapControllers();

app.Run();