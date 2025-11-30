using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(x =>
{

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("notifications", false));

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"] ?? "localhost", "/", h =>
        {
            h.Username(builder.Configuration.GetValue("RabbitMq:Username", "guest")!);
            h.Password(builder.Configuration.GetValue("RabbitMq:Password", "guest")!);
        });

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");

app.Run();
