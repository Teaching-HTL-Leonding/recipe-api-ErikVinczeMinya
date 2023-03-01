using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var nextRecipeId = 0;
var recipes = new ConcurrentDictionary<int, Recipe>();

app.MapPost("/recipes", (CreateOrUpdateRecipeDTO recipeDTO) => {
    var newId = Interlocked.Increment(ref nextRecipeId);

    var recipe = new Recipe{
        Id = nextRecipeId++,
        Title = recipeDTO.Title,
        Ingredients = recipeDTO.Ingredients,
        Desc = recipeDTO.Desc,
        Link = recipeDTO.Link
    };

    if(!recipes.TryAdd(recipe.Id, recipe)){
        return Results.BadRequest("Recipe already exists");
    }
    return Results.Created($"/recipes/{recipe.Id}", recipe);
});

app.MapGet("/recipes", () => recipes.Values);

app.MapDelete("/recipes/{id}", (int id) => {
    if(!recipes.TryRemove(id, out var _)){
        return Results.NotFound();
    }
    return Results.NoContent();
});

app.MapGet("/recipes/filtertitle", (string filter) => {
    var filteredRecipes = recipes.Values.Where(r => r.Title.Contains(filter) || r.Desc.Contains(filter));
    return filteredRecipes;
});

app.MapGet("/recipes/filteringredient", (string filter) => {
    var filteredRecipes = recipes.Values.Where(r => r.Ingredients.Any(i => i.Name.Contains(filter)));
    return filteredRecipes;
});

app.MapPut("/recipes/{id}", (int id, CreateOrUpdateRecipeDTO dto) => {
    if(!recipes.TryGetValue(id, out Recipe? recipe)){
        return Results.NotFound();
    }

    recipe.Title = dto.Title;
    recipe.Ingredients = dto.Ingredients;
    recipe.Desc = dto.Desc;
    recipe.Link = dto.Link;

    return Results.Accepted();
});

app.Run();

class Recipe{
    public int Id {get; set;}
    public string Title {get; set;} = "";
    public Ingredient[] Ingredients {get; set;}
    public string Desc {get; set;} = "";
    public string Link {get; set;} = "";
}

record CreateOrUpdateRecipeDTO(string Title, Ingredient[] Ingredients, string Desc, string Link);

record Ingredient(string Name, int UnitOfMeasure, int Quantity);