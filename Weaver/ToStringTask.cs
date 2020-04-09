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
        // Services imported this way are injected during task construction and can be used during Execute:
        [ImportService]
        private IAnnotationRepositoryService annotationRepositoryService;

        [ImportService] private ICompilerAdapterService compilerAdapterService;

        private Assets assets;

        // This string, if defined, is printed to standard output if you build a project that uses this add-in from commandline.
        // It will not show up in Visual Studio/Rider.
        public override string CopyrightNotice => null;

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

            List<FieldDefDeclaration> fields = new List<FieldDefDeclaration>();
            foreach (FieldDefDeclaration field in enhancedType.Fields)
            {
                if (field.IsStatic || field.IsConst || ignoredDeclarations.Contains(field)) continue;
                fields.Add(field);
            }
            
            List<PropertyDeclaration> properties = new List<PropertyDeclaration>();
            foreach (PropertyDeclaration property in enhancedType.Properties)
            {
                if (property.IsStatic || ignoredDeclarations.Contains(property) || !property.CanRead || property.Getter.Parameters.Count != 0) continue;
                FieldDefDeclaration backingField = compilerAdapterService.GetBackingField(property);
                if (backingField != null)
                {
                    fields.Remove(backingField);
                }
                properties.Add(property);
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
                
                // Return the hash:
                writer.EmitBranchingInstruction(OpCodeNumber.Br, getHashCodeData.ReturnSequence);
                writer.DetachInstructionSequence();
            }
        }

        private void EmitLoadToString(InstructionWriter writer, int index, PropertyDeclaration property, bool enhancedTypeIsValueType)
        {
            EmitPrologueToLoad(writer, index);
            writer.EmitInstruction(OpCodeNumber.Ldarg_0);
            writer.EmitInstructionMethod(enhancedTypeIsValueType ? OpCodeNumber.Call : OpCodeNumber.Callvirt, property.Getter.GetCanonicalGenericInstance());
            EmitEpilogueToLoad(writer, property.PropertyType);
        }
        private void EmitLoadToString(InstructionWriter writer, int index, FieldDefDeclaration field)
        {
            EmitPrologueToLoad(writer, index);
            writer.EmitInstruction(OpCodeNumber.Ldarg_0);
            writer.EmitInstructionField(OpCodeNumber.Ldfld, field.GetCanonicalGenericInstance());
            EmitEpilogueToLoad(writer, field.FieldType);
        }
        private void EmitPrologueToLoad(InstructionWriter writer, int index)
        {
            writer.EmitInstruction(OpCodeNumber.Dup);
            writer.EmitInstructionInt32(OpCodeNumber.Ldc_I4, index);
            // TODO null: write null
        }
        private void EmitEpilogueToLoad(InstructionWriter writer, ITypeSignature type)
        {
            if (type.IsValueTypeSafe() == true || type.TypeSignatureElementKind == TypeSignatureElementKind.GenericParameterReference)
            {
                writer.EmitInstructionType(OpCodeNumber.Box, type);
            }
            writer.EmitInstruction(OpCodeNumber.Stelem_Ref);
        }

      

      

        private string ConstructFormatString(Configuration config, TypeDefDeclaration type, List<FieldDefDeclaration> fields, List<PropertyDeclaration> properties)
        {
            StringBuilder sb = new StringBuilder();
            if (config.WrapWithBraces)
            {
                sb.Append("{{");
            }
            if (config.WriteTypeName)
            {
                sb.Append(type.ShortName + "; ");
            }

            var all = fields.Concat<NamedMetadataDeclaration>(properties);
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