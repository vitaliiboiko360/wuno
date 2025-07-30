using WsAppFile;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var wsApp = new WsApp();

app.UseWebSockets();

app.MapGet("/", () => "Hello World!");

app.MapGet("/ws", wsApp.Main);

app.Run();
