using AiLibrary.Application.Dtos.Chat;
using MediatR;

namespace AiLibrary.Application.Commands;

public record ChatCommand(string? SessionId, string Message) : IRequest<ChatResponse>;
