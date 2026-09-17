using PostService.Dtos;
using PostService.Mappings;
using PostService.BusinessLogic;
using PostService.DataAccess;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IPostingRepository, PostingRepository>();
builder.Services.AddScoped<IPostingService, PostingService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var repository = scope.ServiceProvider.GetRequiredService<IPostingRepository>();
    repository.CreateDb();
}

app.MapGet("/", () => "Це API поштового клієнта");

app.MapGet("/postings", (IPostingService postingService) =>
{
    var postings = postingService.GetAll();
    var resultDtos = new List<PostingGetDto>();
    foreach (var posting in postings)
    {
        resultDtos.Add(PostingMapper.ToPostingGetDto(posting));
    }
    return Results.Ok(resultDtos);
});

app.MapGet("/postings/{id}", (int id, IPostingService postingService) =>
{
    var posting = postingService.Find(id);
    if (posting is null)
    {
        return Results.NotFound();
    }

    var resultDto = PostingMapper.ToPostingGetDto(posting);
    return Results.Ok(resultDto);
});

app.MapPost("/postings", (PostingPostDto postDto, IPostingService postingService) =>
{
    var newPosting = PostingMapper.ToPosting(postDto);
    var savedObject = postingService.Create(newPosting);
    var resultDto = PostingMapper.ToPostingGetDto(savedObject);
    return Results.Created($"/postings/{resultDto.Id}", resultDto);
});

app.MapPut("/postings/{id}", (int id, PostingPutDto putDto, IPostingService postingService) =>
{
    if (id != putDto.Id)
    {
        return Results.BadRequest("Id в маршруті та в тілі запиту не збігаються");
    }

    var postingToUpdate = PostingMapper.ToPosting(putDto);
    var updated = postingService.Update(postingToUpdate);
    if (updated is null)
    {
        return Results.NotFound();
    }

    var resultDto = PostingMapper.ToPostingGetDto(updated);
    return Results.Ok(resultDto);
});

app.MapDelete("/postings/{id}", (int id, IPostingService postingService) =>
{
    var deletedCount = postingService.Delete(id);
    if (deletedCount == 0)
    {
        return Results.NotFound();
    }

    return Results.NoContent();
});

app.Run();