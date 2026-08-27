using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace RoomFlow.Api;

internal sealed class OpenApiServerTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Servers =
        [
            new OpenApiServer
            {
                Url = "/",
                Description = "Current host"
            }
        ];
        return Task.CompletedTask;
    }
}
