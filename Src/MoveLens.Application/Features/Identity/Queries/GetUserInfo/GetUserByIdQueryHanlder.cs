



using MediatR;
using Microsoft.Extensions.Logging;
using MoveLens.Application.Common.Interfaces;
using MoveLens.Application.Features.Identity.Dtos;
using MoveLens.Domain.Common.Results;

namespace MoveLens.Application.Features.Identity.Queries.GetUserInfo;

public class GetUserByIdQueryHanlder(ILogger<GetUserByIdQueryHanlder> logger, IIdentityService identityService)
    : IRequestHandler<GetUserByIdQuery, Result<AppUserDto>>
{
    private readonly ILogger<GetUserByIdQueryHanlder> _logger = logger;
    private readonly IIdentityService _identityService = identityService;

    public async Task<Result<AppUserDto>> Handle(GetUserByIdQuery request, CancellationToken ct)
    {
        var getUserByIdResult = await _identityService.GetUserByIdAsync(request.UserId!);

        if (getUserByIdResult.IsError)
        {
            _logger.LogError("User with Id { UserId }{ErrorDetails}", request.UserId, getUserByIdResult.TopError.Description);

            return getUserByIdResult.Errors;
        }

        return getUserByIdResult.Value;
    }
}