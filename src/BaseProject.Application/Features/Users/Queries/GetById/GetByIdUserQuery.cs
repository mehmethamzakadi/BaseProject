using BaseProject.Domain.Common.Results;
using MediatR;

namespace BaseProject.Application.Features.Users.Queries.GetById;

public sealed record GetByIdUserQuery(Guid Id) : IRequest<IDataResult<GetByIdUserResponse>>;
