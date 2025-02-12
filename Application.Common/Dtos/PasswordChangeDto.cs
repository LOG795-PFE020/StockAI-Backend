namespace Application.Common.Dtos;

public sealed record PasswordChangeDto(string NewPassword, string OldPassword);