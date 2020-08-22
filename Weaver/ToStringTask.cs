using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using PostSharp.Reflection;
using PostSharp.Sdk.CodeModel;
using PostSharp.Sdk.CodeModel.Helpers;
using PostSharp.Sdk.CodeModel.TypeSignatures;
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
        [ImportService]
        private ICompilerAdapterService compilerAdapterService;

        private Assets assets;

        public override string CopyrightNotice => "Simon Cropp, PostSharp Technologies, and contributors";

        public override bool Execute()
        {
            assets = new Assets(this.Project.Module);
            
            List<IAnnotationInstance> types = annotationRepositoryService.GetAnnotations(typeof(ToStringAttribute));
            List<IAnnotationInstance> ignored = annotationRepositoryService.GetAnnotations(typeof(IgnoreDuringToStringAttribute));
            HashSet<MetadataDeclaration> ignoredDeclarations = new HashSet<MetadataDeclaration>(ignored.Select(tuple => tuple.TargetElement));
            var basicConfig = Configuration.FindGlobalConfiguration(annotationRepositoryService);
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

            var (fields, properties) = FindFieldsAndProperties(enhancedType, ignoredDeclarations, config);

            // Generate code:
            using (InstructionWriter writer = InstructionWriter.GetInstance())
            {
                CreatedEmptyMethod getHashCodeData = MethodBodyCreator.CreateModifiableMethodBody(writer, method);
                writer.AttachInstructionSequence(getHashCodeData.PrincipalBlock.AddInstructionSequence());
                
                // Create the format string and put it on the stack:
                int numberOfArguments = fields.Count + properties.Count;
                string formatString = ConstructFormatString(config, enhancedType, fields, properties);
                writer.EmitInstructionString(OpCodeNumber.Ldstr, formatString);
                // Create the argument array and put it on the stack:
                writer.EmitInstructionInt32(OpCodeNumber.Ldc_I4, numberOfArguments);
                writer.EmitInstructionType(OpCodeNumber.Newarr, enhancedType.Module.Cache.GetIntrinsic(IntrinsicType.Object));
                
                // Put all the field and property values on the stack:
                int i = 0;
                foreach (var field in fields)
                {
                    EmitLoadToString(writer, i, field, config);
                    i++;
                }
                bool enhancedTypeIsValueType = enhancedType.IsValueTypeSafe() == true;
                foreach (var property in properties)
                {
                    EmitLoadToString(writer, i, property, enhancedTypeIsValueType, config);
                    i++;
                }
                
                // Return string.Format(formatString, theArgumentArray):
                writer.EmitInstructionMethod(OpCodeNumber.Call, assets.String_Format);
                writer.EmitInstructionLocalVariable(OpCodeNumber.Stloc, getHashCodeData.ReturnVariable);
                writer.EmitBranchingInstruction(OpCodeNumber.Br, getHashCodeData.ReturnSequence);
                writer.DetachInstructionSequence();
            }
        }

        /// <summary>
        /// Finds all fields and properties in the type that should be put into ToString. This includes accessible
        /// fields and properties in base classes, transitively, but it excludes ignored fields and properties.
        /// </summary>
        private (List<UsableField> fields, List<UsableProperty> properties) FindFieldsAndProperties(TypeDefDeclaration enhancedType,
            HashSet<MetadataDeclaration> ignoredDeclarations, Configuration config)
        {
            List<UsableField> fields = new List<UsableField>();
            List<UsableProperty> properties = new List<UsableProperty>();
            TypeDefDeclaration processingType = enhancedType;
            GenericMap mapToGetThere = enhancedType.GetGenericContext();
            bool isInBaseType = false;
            while (true)
            {
                foreach (FieldDefDeclaration field in processingType.Fields)
                {
                    if (field.IsStatic || field.IsConst || ignoredDeclarations.Contains(field)) continue;
                    if (field.Visibility == Visibility.Private && !config.IncludePrivate) continue;
                    // Exclude inaccessible fields:
                    if (isInBaseType && !field.IsVisible(enhancedType)) continue;
                    // Exclude PostSharp and generated fields:
                    if (field.Name[0] == '<') continue;
                    // Exclude field-like events:
                    if (processingType.Events.Any(ev => ev.Name == field.Name)) continue;
                    
                    fields.Add(new UsableField(field, mapToGetThere));
                }

                foreach (PropertyDeclaration property in processingType.Properties)
                {
                    // For auto-implemented properties, consider the property only, not the field:
                    FieldDefDeclaration backingField = compilerAdapterService.GetBackingField(property);
                    if (backingField != null)
                    {
                        fields.RemoveAll(f => f.FieldDefinition == backingField);
                    }
                    // Exclude indexers:
                    if (property.IsStatic || ignoredDeclarations.Contains(property) || !property.CanRead ||
                        property.Getter.Parameters.Count != 0) continue;
                    if (property.Visibility == Visibility.Private && !config.IncludePrivate) continue;
                    // Exclude inaccessible properties:
                    if (isInBaseType && !property.IsVisible(enhancedType)) continue;
                    // Exclude PostSharp and generated fields that were lifted into properties:
                    if (property.Name[0] == '<') continue;
                    // Exclude base properties with the same name. This way, if a property is overridden, we output it
                    // only once:
                    if (properties.Any(prp => prp.PropertyDefinition.Name == property.Name)) continue;
                    // Exclude field-like events (whose fields were promoted to properties by PostSharp):
                    if (processingType.Events.Any(ev => ev.Name == property.Name)) continue;
                    
                    properties.Add(new UsableProperty(property, mapToGetThere));
                }


                // Ends at System.Object:
                if (processingType.BaseType == null)
                {
                    break;
                }

                isInBaseType = true;
                mapToGetThere = processingType.BaseType.GetGenericContext().Apply(mapToGetThere);
                processingType = processingType.BaseType.GetTypeDefinition();
            }

            return (fields, properties);
        }

        private void EmitLoadToString(InstructionWriter writer, int index, UsableProperty property, bool enhancedTypeIsValueType, Configuration config)
        {
            EmitPrologueToLoad(writer, index);
            
            // Load the value:
            writer.EmitInstruction(OpCodeNumber.Ldarg_0);
            writer.EmitInstructionMethod(enhancedTypeIsValueType ? OpCodeNumber.Call : OpCodeNumber.Callvirt,
                property.PropertyDefinition.Getter
                    .GetGenericInstance(property.MapToAccessThisPropertyFromMostDerivedClass)
                    .TranslateMethod(this.Project.Module));
            
            EmitEpilogueToLoad(writer, property.PropertyDefinition.PropertyType.TranslateType(this.Project.Module).MapGenericArguments(property.MapToAccessThisPropertyFromMostDerivedClass), config);
        }
        
        private void EmitLoadToString(InstructionWriter writer, int index, UsableField field, Configuration config)
        {
            EmitPrologueToLoad(writer, index);
            
            // Load the value:
            writer.EmitInstruction(OpCodeNumber.Ldarg_0);
            IField usedField = field.FieldDefinition.Translate(this.Project.Module).GetGenericInstance(field.MapToAccessTheFieldFromMostDerivedClass);
            writer.EmitInstructionField(OpCodeNumber.Ldfld, usedField);
            
            EmitEpilogueToLoad(writer, usedField.FieldType.TranslateType(this.Project.Module).MapGenericArguments(field.MapToAccessTheFieldFromMostDerivedClass), config);
        }
        
        private void EmitPrologueToLoad(InstructionWriter writer, int index)
        {
            writer.EmitInstruction(OpCodeNumber.Dup); // puts a pointer to the argument array on the stack
            writer.EmitInstructionInt32(OpCodeNumber.Ldc_I4, index); // puts an index into the argument array on the stack
        }
        
        private void EmitEpilogueToLoad(InstructionWriter writer, ITypeSignature type, Configuration config)
        {
            if (type.IsValueTypeSafe() == true || type.TypeSignatureElementKind == TypeSignatureElementKind.GenericParameterReference ||
                type.TypeSignatureElementKind == TypeSignatureElementKind.GenericParameter)
            {
                writer.EmitInstructionType(OpCodeNumber.Box, type);
                EmitCollectionTransform(writer, type, config);
               
            }
            else
            {
                writer.EmitInstruction(OpCodeNumber.Dup);
                // if null?
                writer.IfNotZero(() =>
                    {
                        // ok, use the duplicate
                        EmitCollectionTransform(writer, type, config);
                    },
                    () =>
                    {
                        writer.EmitInstruction(OpCodeNumber.Pop); // remove the duplicate
                        writer.EmitInstructionString(OpCodeNumber.Ldstr, "null"); // replace with null
                    });
            }

            // store the value onto the position in the argument array (position was put onto stack by the prologue)
            writer.EmitInstruction(OpCodeNumber.Stelem_Ref);
        }

        private void EmitCollectionTransform(InstructionWriter writer, ITypeSignature type, Configuration config)
        {
            if (CollectionsWeaver.IsCollection(type) || type is ArrayTypeSignature)
            { 
                // ToString a collection first instead of using it directly:
                // The collection is currently on the stack, so we just add the properties separator:
                writer.EmitInstructionString(OpCodeNumber.Ldstr, config.PropertiesSeparator);
                writer.EmitInstructionMethod(OpCodeNumber.Call, assets.CollectionHelper_ToString);
            }
        }

        private string ConstructFormatString(Configuration config, TypeDefDeclaration type, List<UsableField> fields, List<UsableProperty> properties)
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

            var all = fields.Select(fld => fld.FieldDefinition).Concat<NamedMetadataDeclaration>(properties.Select(prp => prp.PropertyDefinition));
            int i = 0;
            foreach (NamedMetadataDeclaration item in all)
            {
                if (i != 0)
                {
                    sb.Append(config.PropertiesSeparator);
                }

                string name = GetPropertyNameToWrite(item.Name, config);
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

        private string GetPropertyNameToWrite(string propertyName, Configuration config)
        {
            string name = propertyName;
            switch (config.PropertyNamingConvention)
            {
                case NamingConvention.CamelCase:
                    return ConvertToCamelCase(name);
                default:
                    return name;
            }
        }

        private static string ConvertToCamelCase(string value)
        {
            if (value == null || value.Length == 0)
            {
                return value;
            }
            return char.ToLower(value[0]) + value.Substring(1);
        }
    }
}