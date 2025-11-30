using BaseProject.Application.Abstractions.Images;
using BaseProject.Domain.Common.Results;
using MediatR;

namespace BaseProject.Application.Features.Images.Commands.Upload;

public sealed record UploadImageCommand(
    byte[] Content,
    string FileName,
    string ContentType,
    long FileSize,
    string Scope,
    ImageResizeMode ResizeMode,
    int? TargetWidth,
    int? TargetHeight,
    string? Title
) : IRequest<IDataResult<UploadImageResponse>>;
