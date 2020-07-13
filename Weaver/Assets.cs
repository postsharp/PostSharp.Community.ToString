using System.Collections.Generic;
using PostSharp.Sdk.CodeModel;

namespace PostSharp.Community.ToString.Weaver
{
    public class Assets
    {
        /// <summary>
        /// Gets the method <see cref="string.Format(string,object[])"/>.
        /// </summary>
        public IMethod String_Format { get; }
        /// <summary>
        /// Gets the method <see cref="CollectionHelper.ToString(System.Collections.IEnumerable)"/>.
        /// </summary>
        public IGenericMethodDefinition CollectionHelper_ToString;

        public Assets(ModuleDeclaration module)
        {
            var stringType = module.Cache.GetType(typeof(string)).GetTypeDefinition();
            String_Format = module.FindMethod(stringType, "Format", (m)=>
                m.Parameters.Count == 2 &&
                m.Parameters[1].ParameterType.TypeSignatureElementKind == TypeSignatureElementKind.Array);    
            var collectionHelperTypeDef = module.Cache.GetType( typeof(CollectionHelper) ).GetTypeDefinition();
            this.CollectionHelper_ToString = module.FindMethod( collectionHelperTypeDef, "ToString",
                declaration => declaration.IsStatic );
        }
    }
}