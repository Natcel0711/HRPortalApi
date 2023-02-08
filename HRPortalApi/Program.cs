var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.Logger.LogInformation("The app started");
app.MapPost("signin", Users.SignInAsync);
app.MapPost("signup", Users.SignUpAsync);
app.MapPost("getbysession", Users.GetUserBySession);

app.Run();
