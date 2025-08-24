using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using AdPlatformLocatorApi.Models;
using AdPlatformLocatorApi.Services;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSingleton<InMemoryPlatformIndex>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();


app.MapPost("/platforms/upload", async Task<Results<Ok<LoadResultDto>, BadRequest<string>>> (
HttpRequest request,
InMemoryPlatformIndex index,
 IFormFile file) =>
{
    if (file == null || file.Length == 0)
        return TypedResults.BadRequest("Upload a non-empty text file in field 'file'.");


    using var stream = file.OpenReadStream();
    using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
    var text = await reader.ReadToEndAsync();


    try
    {
        var result = index.LoadFromText(text);
        return TypedResults.Ok(new LoadResultDto(result.TotalPlatforms, result.TotalLocations));
    }
    catch (Exception ex)
    {
        return TypedResults.BadRequest($"Failed to parse file: {ex.Message}");
    }
}).DisableAntiforgery();


app.MapGet("/platforms", Results<Ok<List<string>>, BadRequest<string>> ( 
[FromQuery] string? location,
InMemoryPlatformIndex index) =>
{
    if (string.IsNullOrWhiteSpace(location))
        return TypedResults.BadRequest("Query parameter 'location' is required, e.g. /platforms?location=/ru/svrd/revda");


    var list = index.FindPlatforms(location!);
    return TypedResults.Ok(list);
}).DisableAntiforgery();


app.Run();
