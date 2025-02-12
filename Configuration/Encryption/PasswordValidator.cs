using Application.Common.Configurations;
using Domain.User;
using Microsoft.AspNetCore.Identity;

namespace Configuration.Encryption;

public sealed class PasswordValidator : IPasswordValidator<UserPrincipal>
{
    private readonly PasswordSettings _passwordSettings;

    public PasswordValidator(PasswordSettings passwordSettings)
    {
        _passwordSettings = passwordSettings;
    }

    public Task<IdentityResult> ValidateAsync(UserManager<UserPrincipal> manager, UserPrincipal user, string? password)
    {
        if (password is null)
        {
            throw new ArgumentNullException(nameof(password));
        }

        var errors = new List<IdentityError>();

        if (password.Length < _passwordSettings.RequiredLength)
        {
            errors.Add(new IdentityError
            {
                Code = "PasswordTooShort",
                Description = $"Passwords must be at least {_passwordSettings.RequiredLength} characters."
            });
        }

        if (_passwordSettings.RequireNonAlphanumeric && password.All(char.IsLetterOrDigit))
        {
            errors.Add(new IdentityError
            {
                Code = "PasswordRequiresNonAlphanumeric",
                Description = "Passwords must have at least one non alphanumeric character."
            });
        }

        if (_passwordSettings.RequireDigit && !password.Any(char.IsDigit))
        {
            errors.Add(new IdentityError
            {
                Code = "PasswordRequiresDigit",
                Description = "Passwords must have at least one digit ('0'-'9')."
            });
        }

        if (_passwordSettings.RequireLowercase && !password.Any(char.IsLower))
        {
            errors.Add(new IdentityError
            {
                Code = "PasswordRequiresLower",
                Description = "Passwords must have at least one lowercase ('a'-'z')."
            });
        }

        if (_passwordSettings.RequireUppercase && !password.Any(char.IsUpper))
        {
            errors.Add(new IdentityError
            {
                Code = "PasswordRequiresUpper",
                Description = "Passwords must have at least one uppercase ('A'-'Z')."
            });
        }

        return Task.FromResult(errors.Count == 0
                       ? IdentityResult.Success
                                  : IdentityResult.Failed(errors.ToArray()));
    }
}