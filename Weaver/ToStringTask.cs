using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using PostSharp.Sdk.CodeModel;
using PostSharp.Sdk.CodeModel.Helpers;
using PostSharp.Sdk.CodeModel.TypeSignatures;
using PostSharp.Sdk.Collections;
using PostSharp.Sdk.Extensibility;
using PostSharp.Sdk.Extensibility.Compilers;
using PostSharp.Sdk.Extensibility.Tasks;

namespace PostSharp.Community.ToString.Weaver
{

    [ExportTask(Phase = TaskPhase.CustomTransform, TaskName = nameof(ToStringTask))]
    public class ToStringTask : Task
    {
        [ImportService]
        private IAnnotationRepositoryService annotationRepositoryService;
        [ImportService] private ICompilerAdapterService compilerAdapterService;

        private Assets assets;

        public override string CopyrightNotice => "Simon Cropp, PostSharp Technologies, and contributors";

        public override bool Execute()
        {
            assets = new Assets(this.Project.Module);
            
            List<IAnnotationInstance> types = annotationRepositoryService.GetAnnotations(typeof(ToStringAttribute));
            List<IAnnotationInstance> ignored = annotationRepositoryService.GetAnnotations(typeof(IgnoreDuringToStringAttribute));
            List<IAnnotationInstance> global = annotationRepositoryService.GetAnnotations(typeof(ToStringGlobalOptionsAttribute));
            Configuration basicConfig = new Configuration();
            if (global.Count > 0)
            {
                basicConfig = Configuration.ReadConfiguration(global[0].Value, basicConfig);
            }

            HashSet<MetadataDeclaration> ignoredDeclarations = new HashSet<MetadataDeclaration>(ignored.Select(tuple => tuple.TargetElement));
            foreach (var tuple in types)
            {
                var config = Configuration.ReadConfiguration(tuple.Value, basicConfig);
                AddToStringToType(tuple.TargetElement as TypeDefDeclaration, config, ignoredDeclarations);
            }
            return true;
        }

        private void AddToStringToType(TypeDefDeclaration enhancedType, Configuration config, HashSet<MetadataDeclaration> ignoredDeclarations)
        {
            if (enhancedType.Methods.Any<IMethod>(m => m.Name == "ToString" &&
                                                               !m.IsStatic && 
                                                               m.ParameterCount == 0))
            {
                // It's already present, just skip it.
                return;
            }
               
            // Create signature
            MethodDefDeclaration method = new MethodDefDeclaration
            {
                Name = "ToString",
                CallingConvention = CallingConvention.HasThis,
                Attributes = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig
            };
            enhancedType.Methods.Add(method);
            CompilerGeneratedAttributeHelper.AddCompilerGeneratedAttribute(method);
            method.ReturnParameter = ParameterDeclaration.CreateReturnParameter(enhancedType.Module.Cache.GetIntrinsic(IntrinsicType.String));

            List<UsableField> fields = new List<UsableField>();
            List<PropertyDeclaration> properties = new List<PropertyDeclaration>();
            TypeDefDeclaration processingType = enhancedType;
            GenericMap mapToGetThere = enhancedType.GetGenericContext();
            while (true)
            {
                foreach (FieldDefDeclaration field in processingType.Fields)
                {
                    if (field.IsStatic || field.IsConst || ignoredDeclarations.Contains(field)) continue;
                    fields.Add(new UsableField(field, mapToGetThere));
                }
                foreach (PropertyDeclaration property in processingType.Properties)
                {
                    FieldDefDeclaration backingField = compilerAdapterService.GetBackingField(property);
                    if (backingField != null)
                    {
                        fields.RemoveAll(f => f.FieldDefinition == backingField);
                    }

                    if (property.IsStatic || ignoredDeclarations.Contains(property) || !property.CanRead ||
                        property.Getter.Parameters.Count != 0) continue;
                    properties.Add(property);
                }

                if (processingType.BaseType == null)
                {
                    break;
                }
                mapToGetThere = processingType.BaseType.GetGenericContext().Apply(mapToGetThere);
                processingType = processingType.BaseType.GetTypeDefinition();
            }

            // Generate code:
            using (InstructionWriter writer = InstructionWriter.GetInstance())
            {
                CreatedEmptyMethod getHashCodeData = MethodBodyCreator.CreateModifiableMethodBody(writer, method);
                var resultVariable = getHashCodeData.ReturnVariable;
                writer.AttachInstructionSequence(getHashCodeData.PrincipalBlock.AddInstructionSequence());

                bool enhancedTypeIsValueType = enhancedType.IsValueTypeSafe() == true;
                int numberOfArguments = fields.Count + properties.Count;
                string formatString = ConstructFormatString(config, enhancedType, fields, properties);
                writer.EmitInstructionString(OpCodeNumber.Ldstr, formatString);
                 writer.EmitInstructionInt32(OpCodeNumber.Ldc_I4, numberOfArguments);
                 writer.EmitInstructionType(OpCodeNumber.Newarr, enhancedType.Module.Cache.GetIntrinsic(IntrinsicType.Object));
                int i = 0;
                foreach (var field in fields)
                {
                    EmitLoadToString(writer, i, field);
                    i++;
                }
                foreach (var property in properties)
                {
                    EmitLoadToString(writer, i, property, enhancedTypeIsValueType);
                    i++;
                }
                writer.EmitInstructionMethod(OpCodeNumber.Call, assets.String_Format);
                writer.EmitInstructionLocalVariable(OpCodeNumber.Stloc, resultVariable);
                
                writer.EmitBranchingInstruction(OpCodeNumber.Br, getHashCodeData.ReturnSequence);
                writer.DetachInstructionSequence();
            }
        }

        private void EmitLoadToString(InstructionWriter writer, int index, PropertyDeclaration property, bool enhancedTypeIsValueType)
        {
            EmitPrologueToLoad(writer, index);
            writer.EmitInstruction(OpCodeNumber.Ldarg_0);
            writer.EmitInstructionMethod(enhancedTypeIsValueType ? OpCodeNumber.Call : OpCodeNumber.Callvirt,
                property.Getter
                    .GetGenericInstance(property.DeclaringType.GetGenericContext())
                    .TranslateMethod(this.Project.Module));
            EmitEpilogueToLoad(writer, property.PropertyType);
        }
        private void EmitLoadToString(InstructionWriter writer, int index, UsableField field)
        {
            EmitPrologueToLoad(writer, index);
            writer.EmitInstruction(OpCodeNumber.Ldarg_0);
            IField usedField = field.FieldDefinition.Translate(this.Project.Module).GetGenericInstance(field.MapToAccessTheFieldFromMostDerivedClass);
            writer.EmitInstructionField(OpCodeNumber.Ldfld, usedField);
            EmitEpilogueToLoad(writer, usedField.FieldType.TranslateType(this.Project.Module).MapGenericArguments(field.MapToAccessTheFieldFromMostDerivedClass));
        }
        private void EmitPrologueToLoad(InstructionWriter writer, int index)
        {
            writer.EmitInstruction(OpCodeNumber.Dup);
            writer.EmitInstructionInt32(OpCodeNumber.Ldc_I4, index);
        }
        private void EmitEpilogueToLoad(InstructionWriter writer, ITypeSignature type)
        {
            if (type.IsValueTypeSafe() == true || type.TypeSignatureElementKind == TypeSignatureElementKind.GenericParameterReference)
            {
                writer.EmitInstructionType(OpCodeNumber.Box, type);
            }
            else
            {
                writer.EmitInstruction(OpCodeNumber.Dup);
                // if null?
                writer.IfNotZero(() =>
                    {
                        // ok, use the duplicate
                    },
                    () =>
                    {
                        writer.EmitInstruction(OpCodeNumber.Pop); // remove the duplicate
                        writer.EmitInstructionString(OpCodeNumber.Ldstr, "null"); // replace with null
                    });
            }

            writer.EmitInstruction(OpCodeNumber.Stelem_Ref);
        }

      

      

        private string ConstructFormatString(Configuration config, TypeDefDeclaration type, List<UsableField> fields, List<PropertyDeclaration> properties)
        {
            StringBuilder sb = new StringBuilder();
            bool isThereAnything = fields.Count > 0 || properties.Count > 0;
            if (config.WrapWithBraces)
            {
                sb.Append("{{");
            }
            if (config.WriteTypeName)
            {
                sb.Append(type.ShortName + (isThereAnything ? "; " : ""));
            }

            var all = fields.Select(fld => fld.FieldDefinition).Concat<NamedMetadataDeclaration>(properties);
            int i = 0;
            foreach (NamedMetadataDeclaration item in all)
            {
                if (i != 0)
                {
                    sb.Append(config.PropertiesSeparator);
                }

                string name = item.Name;
                int lastDot = name.LastIndexOf('.');
                if (lastDot != -1)
                {
                    name = name.Substring(lastDot + 1);
                }
                sb.Append(name);
                sb.Append(config.NameValueSeparator);
                sb.Append("{");
                sb.Append(i);
                sb.Append("}");
                i++;
            }
            if (config.WrapWithBraces)
            {
                sb.Append("}}");
            }
            return sb.ToString();
        }
    }
}