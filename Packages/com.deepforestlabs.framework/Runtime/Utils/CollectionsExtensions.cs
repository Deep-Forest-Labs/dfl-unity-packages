#nullable enable
namespace System.Collections.Generic
{
    public static class CollectionsExtensions
    {
        public static int IndexOf<T>(this IReadOnlyList<T> list, T value)
        {
            var comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < list.Count; i++)
            {
                if (comparer.Equals(list[i], value))
                    return i;
            }
            return -1;
        }
    }
}