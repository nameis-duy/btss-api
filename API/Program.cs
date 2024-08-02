using API;
using Hangfire;
using Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;
using Sentry.Extensibility;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseSentry(opt =>
{
    opt.Dsn = builder.Configuration["Sentry:Dsn"];
    opt.SendDefaultPii = true;
    opt.MaxRequestBodySize = RequestSize.Always;
    opt.MinimumBreadcrumbLevel = LogLevel.Debug;
    opt.MinimumEventLevel = LogLevel.Warning;
    opt.AttachStacktrace = true;
    opt.TracesSampleRate = 1;
    opt.DiagnosticLevel = SentryLevel.Error;
});
// Add services to the container.
builder.Services.AddInfra(builder.Configuration);
builder.Services.AddWebService(builder.Configuration);
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGraphQL();
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    DashboardTitle = "BTSS Background Jobs",
    Authorization = [new HangfireAuthFilter()]
});

// Start background job
//using (var scope = app.Services.CreateScope())
//{
//    var bgJobService = scope.ServiceProvider.GetRequiredService<IBackgroundService>();
//    bgJobService.RecurNotifyPlanDepart();
//};

app.Run();