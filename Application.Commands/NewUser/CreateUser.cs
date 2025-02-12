using Application.Commands.Seedwork;

namespace Application.Commands.NewUser;

public sealed record CreateUser(string Username, string Password, string Role) : ICommand;