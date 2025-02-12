namespace Application.Common.Dtos;

public sealed record AdminChangePasswordDto(string UserName, string NewPassword, string OldPassword);