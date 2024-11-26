using Microsoft.AspNetCore.Http.HttpResults;
namespace eShop.Catalog.API;

public static class CatalogBrandApi
{
    public static IEndpointRouteBuilder MapCatalogBrandApiV1(this IEndpointRouteBuilder app)
    {
        //var api = app.MapGroup("api/catalogbrand").HasApiVersion(1.0);

        //// Routes for querying brands.
        //api.MapGet("/items", GetAllItems);
        //api.MapGet("/items/{id:int}", GetItemById);

        //// Routes for modifying brands.
        //api.MapPut("/items", UpdateItem);
        //api.MapPost("/items", CreateItem);

        return app;
    }

    public static async Task<Results<Ok<PaginatedItems<CatalogBrand>>, BadRequest<string>>> GetAllItems(
        [AsParameters] PaginationRequest paginationRequest,
        [AsParameters] CatalogServices services)
    {
        var pageSize = paginationRequest.PageSize;
        var pageIndex = paginationRequest.PageIndex;

        var totalItems = await services.Context.CatalogBrands
                                       .LongCountAsync();

        var itemsOnPage = await services.Context.CatalogBrands
                                        .OrderBy(c => c.Brand)
                                        .Skip(pageSize * pageIndex)
                                        .Take(pageSize)
                                        .ToListAsync();

        return TypedResults.Ok(new PaginatedItems<CatalogBrand>(pageIndex, pageSize, totalItems, itemsOnPage));
    }
    
    public static async Task<Results<Ok<CatalogBrand>, NotFound, BadRequest<string>>> GetItemById(
        [AsParameters] CatalogServices services,
        int id)
    {
        if (id <= 0)
        {
            return TypedResults.BadRequest("Id is not valid.");
        }

        var item = await services.Context.CatalogBrands.SingleOrDefaultAsync(ci => ci.Id == id);

        if (item == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(item);
    }
    
    public static async Task<Results<Created, NotFound<string>>> UpdateItem(
        [AsParameters] CatalogServices services,
        CatalogBrand catalogBrandToUpdate)
    {
        var catalogBrandItem = await services.Context.CatalogBrands.SingleOrDefaultAsync(i => i.Id == catalogBrandToUpdate.Id);

        if (catalogBrandItem == null)
        {
            return TypedResults.NotFound($"Item with id {catalogBrandToUpdate.Id} not found.");
        }

        // Update current product
        var catalogBrandEntry = services.Context.Entry(catalogBrandItem);
        catalogBrandEntry.CurrentValues.SetValues(catalogBrandToUpdate);

        //catalogItem.Embedding = await services.CatalogAI.GetEmbeddingAsync(catalogItem);

        return TypedResults.Created($"/api/catalogbrand/items/{catalogBrandToUpdate.Id}");
    }
    
    public static async Task<Created> CreateItem(
        [AsParameters] CatalogServices services,
        CatalogBrand catalogBrand)
    {
        var item = new CatalogBrand
        {
            Brand = catalogBrand.Brand
        };

        services.Context.CatalogBrands.Add(item);
        await services.Context.SaveChangesAsync();

        return TypedResults.Created($"/api/catalogbrand/items/{item.Id}");
    }
}
