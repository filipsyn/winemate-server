using MassTransit;

using Microsoft.EntityFrameworkCore;

using WineMate.Contracts.Messages;
using WineMate.Identity.Database;

namespace WineMate.Identity.Features.Authorization;

public class GetUserAdminStatusConsumer : IConsumer<GetUserAdminStatusRequest>
{
    private readonly ApplicationDbContext _dbContext;

    public GetUserAdminStatusConsumer(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<GetUserAdminStatusRequest> context)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == context.Message.UserId);

        if (user is null)
        {
            await context.RespondAsync(new GetUserAdminStatusResponse
            {
                UserId = context.Message.UserId,
                Exists = false,
                IsAdmin = false
            });
            return;
        }

        var response = new GetUserAdminStatusResponse
        {
            UserId = context.Message.UserId,
            IsAdmin = user.IsAdmin
        };

        await context.RespondAsync(response);
    }
}
