using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PostSharp.Community.ToString
{
    /// <summary>
    /// Helper static class used by code synthesized by <see cref="ToStringAttribute"/>.
    /// </summary>
    public static class CollectionHelper
    {
        /// <summary>
        /// Returns the string representation of the first couple of elements of a collection. For example,
        /// for the collection of "1", "2", ... "100", this would return <c>[1,2,3,4,...]</c>.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <returns>The string representation of the collection.</returns>
        public static string ToString( IEnumerable collection )
        {
            if ( collection == null )
            {
                return "null";
            }
            var enumerator = collection.GetEnumerator();
            StringBuilder sb = new StringBuilder();
            sb.Append('[');
            bool first = true;
            int count = 0;
            while ( enumerator.MoveNext() )
            {
                if (count == 4)
                {
                    // Only show the first four elements of the collection.
                    sb.Append(",...");
                    break;
                }
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.Append(',');
                }
                sb.Append(enumerator.Current?.ToString() ?? "null");
                count++;
            }
            sb.Append(']');
            return sb.ToString();
        }
    }
}