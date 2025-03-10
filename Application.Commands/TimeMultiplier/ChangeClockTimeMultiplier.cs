using Application.Commands.Seedwork;

namespace Application.Commands.TimeMultiplier;

public sealed record ChangeClockTimeMultiplier(int Multiplier) : ICommand;