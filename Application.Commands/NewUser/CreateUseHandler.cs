using Application.Commands.Seedwork;
using Domain.Common.Monads;
using Domain.User;
using Microsoft.AspNetCore.Identity;

namespace Application.Commands.NewUser;

public sealed class CreateUseHandler : ICommandHandler<CreateUser>
{
    private readonly UserManager<UserPrincipal> _userManager;

    public CreateUseHandler(UserManager<UserPrincipal> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result> Handle(CreateUser command, CancellationToken cancellation)
    {
        var identity = await _userManager.CreateAsync(new UserPrincipal
        {
            UserName = command.Username,
        }, command.Password);

        if (!identity.Succeeded)
        {
            return Result.Failure(identity.Errors.First().Description);
        }

        var user = await _userManager.FindByNameAsync(command.Username);

        if (user == null)
        {
            return Result.Failure("User not found");
        }

        await _userManager.AddToRoleAsync(user, command.Role);

        return Result.Success();
    }
}