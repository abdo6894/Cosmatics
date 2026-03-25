using MediatR;
using MoveLens.Domain.Common.Results;

namespace MoveLens.Application.Features.Users.Commands.Deactivate;

public sealed record DeactivateUserCommand(Guid UserId) : IRequest<Result<Deleted>>;