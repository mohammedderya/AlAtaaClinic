using AlAtaaClinic.Application.Common.Messaging;

namespace AlAtaaClinic.Application.Features.Auth;

public sealed record GetUserAccountByIdQuery(long Id) : IQuery<UserAccountDto>;
