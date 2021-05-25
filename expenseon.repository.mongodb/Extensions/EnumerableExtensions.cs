namespace ExpenseOn.Repository.MongoDB
{
    using System.Collections.Generic;

    internal static class EnumerableExtensions
    {
        public static IEnumerable<T[]> BatchesOf<T>(this IEnumerable<T> source, int batchSize)
        {
            var batch = new List<T>(batchSize);

            foreach (var item in source)
            {
                batch.Add(item);

                if (batch.Count < batchSize) continue;

                yield return batch.ToArray();

                batch.Clear();
            }

            if (batch.Count <= 0) yield break;

            yield return batch.ToArray();

            batch.Clear();
        }
    }
}
