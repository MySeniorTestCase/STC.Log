using STC.Logger.Infrastructure;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddInfrastructureDependencies(configuration: builder.Configuration);

WebApplication app = builder.Build();

app.Run();