using MediatR;
using MoveLens.Application.Features.Users.Dtos;
using MoveLens.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Text;


namespace MoveLens.Application.Features.Users.Commands.Register
{

    public sealed record RegisterUserCommand(
        string FullName,
        string Email,
        string Password
    ) : IRequest<Result<RegisterUserResponse>>;



































































































}
