namespace ExpenseOn.Repository.MongoDB
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using global::MongoDB.Driver;

    internal static class FindFluentExtensions
    {
        internal static IOrderedFindFluent<TDocument, TDocument> OrderBy<TDocument>(this IFindFluent<TDocument, TDocument> find, (Expression<Func<TDocument, object>> selector, SortDirection direction) orderBy) where TDocument : class
        {
            var (selector, direction) = orderBy;

            switch (direction)
            {
                case SortDirection.Ascending:
                    return find.SortBy(selector);
                case SortDirection.Descending:
                    return find.SortByDescending(selector);
                default:
                    throw new ArgumentOutOfRangeException(nameof(orderBy));
            }
        }

        internal static IOrderedFindFluent<TDocument, TDocument> OrderBy<TDocument>(this IFindFluent<TDocument, TDocument> find, params (Expression<Func<TDocument, object>> selector, SortDirection direction)[] orderByList) where TDocument : class
        {
            return (IOrderedFindFluent<TDocument, TDocument>)orderByList.Aggregate(find, (current, orderBy) => current.OrderBy(orderBy));
        }
    }
}
