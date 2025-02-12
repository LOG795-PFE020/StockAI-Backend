using Application.Commands.Seedwork;

namespace Application.Commands.Password;

public sealed record ChangePassword(string Username, string OldPassword, string NewPassword) : ICommand
{
    public string GetCommandName()
    {
        return nameof(ChangePassword);
    }
}