using MassTransit;

using Microsoft.EntityFrameworkCore;

using WineMate.Catalog.Database;
using WineMate.Contracts.Messages;

namespace WineMate.Catalog.Features.Wines;

public class GetWineStatusConsumer : IConsumer<GetWineStatusRequest>
{
    private readonly ApplicationDbContext _dbContext;

    public GetWineStatusConsumer(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<GetWineStatusRequest> context)
    {
        var wineExists = await _dbContext.Wines
            .AsNoTracking()
            .AnyAsync(wine => wine.Id == context.Message.WineId);

        var response = new GetWineStatusResponse
        {
            WineId = context.Message.WineId,
            Exists = wineExists
        };

        await context.RespondAsync(response);
    }
}
