namespace Domain.Portfolio.ValueObjects;

public record Money(decimal Value)
{
    public static Money operator +(Money left, Money right) => new(left.Value + right.Value);
    public static Money operator -(Money left, Money right) => new(left.Value - right.Value);
    public static Money operator *(Money left, decimal right) => new(left.Value * right);
    public static Money operator /(Money left, decimal right) => new(left.Value / right);
    public static bool operator <(Money left, Money right) => left.Value < right.Value;
    public static bool operator >(Money left, Money right) => left.Value > right.Value;
    public static bool operator <=(Money left, Money right) => left.Value <= right.Value;
    public static bool operator >=(Money left, Money right) => left.Value >= right.Value;
    public override string ToString() => Value.ToString("C");
    public override int GetHashCode() => Value.GetHashCode();

    public bool CanAfford(Money price) => Value >= price.Value;

    public bool CannotAfford(Money price) => !CanAfford(price);

    public static Money Zero => new(0);
}