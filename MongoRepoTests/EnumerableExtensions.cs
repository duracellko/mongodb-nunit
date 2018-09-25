using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace MongoRepoTests
{
    public static class EnumerableExtensions
    {
        public static IAsyncCursor<T> ToAsyncCursor<T>(this IEnumerable<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return new AsyncCursor<T>(source.ToList());
        }

        public static IAsyncCursor<T> ToAsyncCursor<T>(this IEnumerable<T> source, Expression<Func<T, bool>> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var result = source.AsQueryable().Where(predicate).ToList();
            return new AsyncCursor<T>(result);
        }

        private class AsyncCursor<T> : IAsyncCursor<T>
        {
            private readonly IEnumerable<T> _current;
            private bool _moved;

            public AsyncCursor(IEnumerable<T> source)
            {
                _current = source;
            }

            public IEnumerable<T> Current => _moved ? _current : null;

            public void Dispose()
            {
                // Nothing to dispose
            }

            public bool MoveNext(CancellationToken cancellationToken = default(CancellationToken))
            {
                if (_moved)
                {
                    return false;
                }

                _moved = true;
                return true;
            }

            public Task<bool> MoveNextAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.FromResult(MoveNext(cancellationToken));
            }
        }
    }
}
