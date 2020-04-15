using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using PostSharp.Sdk.CodeModel;
using PostSharp.Sdk.Extensibility.Tasks;

namespace PostSharp.Community.ToString.Weaver
{
    public static class Extensions
    {
        /// <summary>
        /// Gets the list of annotations of an attribute <paramref name="type"/> as a list rather than an enumerator.
        /// </summary>
        public static List<IAnnotationInstance> GetAnnotations(
            this IAnnotationRepositoryService service, Type type)
        {
            var l = new List<IAnnotationInstance>();
            var enumerator = service.GetAnnotationsOfType(type, false, true);
            while (enumerator.MoveNext())
            {
                IAnnotationInstance instance = enumerator.Current;
                l.Add(instance);
            }
            return l;
        }
    }
}