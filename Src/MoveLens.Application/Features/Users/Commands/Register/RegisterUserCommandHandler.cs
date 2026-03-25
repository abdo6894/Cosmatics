
using AutoMapper;
using MediatR;
using MoveLens.Application.Common.Interfaces;
using MoveLens.Application.Features.Users.Dtos;
using MoveLens.Domain.Common.Results;
using MoveLens.Domain.Users.Abstraction;
using MoveLens.Domain.Users.ValueObjects;
using DomainUser = MoveLens.Domain.Users.Entities.User;

namespace MoveLens.Application.Features.Users.Commands.Register;

public sealed class RegisterUserCommandHandler(
    IIdentityService identityService,
    IUserRepository userRepository,
    IAppDbContext context,  
    IMapper mapper)
    : IRequestHandler<RegisterUserCommand, Result<RegisterUserResponse>>
{
    public async Task<Result<RegisterUserResponse>> Handle(
        RegisterUserCommand command,
        CancellationToken ct)
    {
        var identityResult = await identityService.CreateAsync(command.Email, command.Password, ct);
        if (identityResult.IsError)
            return identityResult.Errors;

        var userResult = DomainUser.Create(
            command.FullName,
            identityResult.Value.UserId,
            UserPreferences.Default);

        if (userResult.IsError)
            return userResult.Errors;

        await userRepository.AddAsync(userResult.Value, ct);
        await context.SaveChangesAsync(ct);  

        return mapper.Map<RegisterUserResponse>(userResult.Value);
    }
}