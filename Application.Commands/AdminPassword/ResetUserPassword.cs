using Application.Commands.Seedwork;

namespace Application.Commands.AdminPassword;

public sealed record ResetUserPassword(string Username, string Password) : ICommand;