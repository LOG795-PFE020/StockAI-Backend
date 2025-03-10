namespace Domain.Portfolio.ValueObjects;

public record ShareVolume(string Symbol, int Volume)
{
    public static ShareVolume operator +(ShareVolume left, ShareVolume right) => left with { Volume = left.Volume + right.Volume };
    public static ShareVolume operator +(ShareVolume left, int volume) => left with { Volume = left.Volume + volume };
    public static ShareVolume operator -(ShareVolume left, ShareVolume right) => left with { Volume = left.Volume - right.Volume };
    public static ShareVolume operator -(ShareVolume left, int volume) => left with { Volume = left.Volume - volume };
    public static bool operator <(ShareVolume left, ShareVolume right) => left.Volume < right.Volume;
    public static bool operator <(ShareVolume left, int right) => left.Volume < right;
    public static bool operator >(ShareVolume left, ShareVolume right) => left.Volume > right.Volume;
    public static bool operator >(ShareVolume left, int right) => left.Volume > right;
    public static bool operator <=(ShareVolume left, ShareVolume right) => left.Volume <= right.Volume;
    public static bool operator >=(ShareVolume left, ShareVolume right) => left.Volume >= right.Volume;
    public override string ToString() => $"{Symbol}: {Volume}";
    public override int GetHashCode() => HashCode.Combine(Symbol, Volume);
}