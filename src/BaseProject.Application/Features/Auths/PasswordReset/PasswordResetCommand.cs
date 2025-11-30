using BaseProject.Domain.Common.Results;
using MediatR;

namespace BaseProject.Application.Features.Auths.PasswordReset;

public sealed record PasswordResetCommand(string Email) : IRequest<IResult>;

