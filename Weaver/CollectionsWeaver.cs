using System.Collections;
using System.Collections.Generic;
using PostSharp.Sdk.CodeModel;

namespace PostSharp.Community.ToString.Weaver
{
    public static class CollectionsWeaver
    {
        /// <summary>
        /// Returns true if t<paramref name="type"/> is a non-string enumerable type.
        /// </summary>
        public static bool IsCollection(ITypeSignature type)
        {
            return type.IsAssignableToRuntimeType(typeof(IEnumerable)) &&
                   !type.IsAssignableToRuntimeType(typeof(string));
        }
    }
}