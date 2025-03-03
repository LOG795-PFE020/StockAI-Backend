using System.Collections.Immutable;
using Domain.Common.Monads;
using Domain.Common.Seedwork.Abstract;
using Domain.Stock.ValueObjects;

namespace Domain.Stock
{
    public sealed class Share : Aggregate<Share>
    {
        public string Symbol => Id;

        public ImmutableList<Quote> Quotes
        {
            get => _quotes;
            private set => _quotes = value;
        }

        private ImmutableList<Quote> _quotes = [];

        public Share(string id) : base(id)
        {
        }

        public void AddQuote(DateTime day, decimal price)
        {
            ImmutableInterlocked.Update(ref _quotes, list => list.Add(new Quote(day.Date, price)));
        }

        public Result<decimal> GetPrice(DateTime dateTime)
        {
            var quote = Quotes.FirstOrDefault(q => q.Day == dateTime.Date);

            if (quote is null)
            {
                return Result.Failure<decimal>($"Quote not found for {Symbol} at {dateTime}");
            }

            return Result.Success(quote.Price);
        }
    }
}
