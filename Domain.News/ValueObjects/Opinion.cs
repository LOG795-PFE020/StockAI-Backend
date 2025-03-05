namespace Domain.News.ValueObjects;

public record Opinion
{
    public int Value { get; }

    private Opinion(int value)
    {
        Value = value;
    }

    public static Opinion WithPositiveOpinion() => new(1);

    public static Opinion WithNegativeOpinion() => new(-1);

    public static Opinion WithNeutralOpinion() => new(0);

    public Opinion Combine(Opinion opinion)
    {
        if (Value == 0)
        {
            return opinion;
        }
        if (opinion.Value == 0)
        {
            return this;
        }
        if (Value == opinion.Value)
        {
            return this;
        }

        return WithNeutralOpinion();
    }
};