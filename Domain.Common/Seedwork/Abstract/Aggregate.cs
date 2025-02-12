namespace Domain.Common.Seedwork.Abstract;

public abstract class Aggregate<T> : Entity<T> where T : class
{
    protected Aggregate(string id) : base(id)
    {
        
    }
}