using Application.Queries.Seedwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.News;
using Domain.Common.Monads;

namespace Application.Queries.RelevantNews
{
    public class RelevantNewsHandler : IQueryHandler<RelevantNews, SimpleNews>
    {
        public Task<Result<SimpleNews>> Handle(RelevantNews query, CancellationToken cancellation)
        {
            return Task.FromResult(Result.Success(SimpleNews.GetEmpty()));
        }
    }
}
