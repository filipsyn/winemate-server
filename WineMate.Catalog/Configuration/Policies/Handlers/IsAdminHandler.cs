using System.Security.Claims;

using MassTransit;

using Microsoft.AspNetCore.Authorization;

using WineMate.Catalog.Configuration.Policies.Requirements;
using WineMate.Contracts.Messages;

namespace WineMate.Catalog.Configuration.Policies.Handlers;

public class IsAdminHandler : AuthorizationHandler<IsAdminRequirement>
{
    private readonly IRequestClient<GetUserAdminStatusRequest> _requestClient;

    public IsAdminHandler(IRequestClient<GetUserAdminStatusRequest> requestClient)
    {
        _requestClient = requestClient;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        IsAdminRequirement requirement)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
        {
            context.Fail();
            return;
        }

        var response = await _requestClient.GetResponse<GetUserAdminStatusResponse>(new
        {
            UserId = Guid.Parse(userId)
        });

        if (response.Message.IsAdmin)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}
