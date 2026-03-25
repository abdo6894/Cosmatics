using AutoMapper;
using MoveLens.Application.Features.Users.Dtos;
using MoveLens.Domain.Users.ValueObjects;

namespace MoveLens.Application.Features.User.Mappers;

using DomainUser = MoveLens.Domain.Users.Entities.User;


public sealed class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<DomainUser, RegisterUserResponse>()
            .ConstructUsing(src => new RegisterUserResponse(
                src.Id,
                src.FullName,
                src.IdentityId,
                src.IsActive));


        CreateMap<UserPreferences, UserPreferencesResponse>()
            .ConstructUsing(src => new UserPreferencesResponse(
                src.PreferredMoods.ToList(),
                src.Language,
                src.MaxBudget,
                src.FavoriteGovernorates.ToList()));

        CreateMap<DomainUser, UserProfileResponse>()
            .ConstructUsing((src, ctx) => new UserProfileResponse(
                src.Id,
                src.FullName,
                src.IdentityId,
                src.IsActive,
                ctx.Mapper.Map<UserPreferencesResponse>(src.Preferences))); // استخدم ctx.Mapper
    }
}