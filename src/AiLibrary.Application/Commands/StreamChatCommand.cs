using AiLibrary.Application.Dtos.Chat;
using MediatR;

namespace AiLibrary.Application.Commands;

public record StreamChatCommand(string? SessionId, string Message) : IRequest<StreamChatResult>;
