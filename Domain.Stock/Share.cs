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
            ImmutableInterlocked.Update(ref _quotes, list =>
            {
                list = list.Add(new Quote(day.Date, price));
                return list.OrderBy(quote => quote.Day).ToImmutableList();
            });
        }

        public Result<decimal> GetPrice(DateTime dateTime)
        {
            if (Quotes.Count == 0)
            {
                return Result.Failure<decimal>("No quotes available");
            }

            int currentPriceIndex;

            var firstFuture = Quotes.FindIndex(q => q.Day > dateTime.Date);

            // If there are no future quotes, the current price is the last one
            if (firstFuture == -1)
            {
                currentPriceIndex = Quotes.Count - 1;
            }
            // If there are future quotes, the current price is the one before the first future quote
            else
            {
                currentPriceIndex = firstFuture - 1;
            }

            var quote = Quotes[currentPriceIndex];

            return Result.Success(quote.Price);
        }
    }
}
