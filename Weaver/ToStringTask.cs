using System;
using System.Collections.Generic;
using System.Reflection;
using PostSharp.Community.ToString;
using PostSharp.Extensibility;
using PostSharp.Reflection;
using PostSharp.Sdk.CodeModel;
using PostSharp.Sdk.CodeModel.TypeSignatures;
using PostSharp.Sdk.Extensibility;
using PostSharp.Sdk.Extensibility.Configuration;
using PostSharp.Sdk.Extensibility.Tasks;

namespace PostSharp.Community.HelloWorld.Weaver
{

    [ExportTask(Phase = TaskPhase.CustomTransform, TaskName = nameof(ToStringTask))]
    public class ToStringTask : Task
    {
        // Services imported this way are injected during task construction and can be used during Execute:
        [ImportService]
        private IAnnotationRepositoryService annotationRepositoryService;

        // This string, if defined, is printed to standard output if you build a project that uses this add-in from commandline.
        // It will not show up in Visual Studio/Rider.
        public override string CopyrightNotice => null;

        public override bool Execute()
        {
            return true;
            var consoleWriteLine = FindConsoleWriteLine();

            var enumerator =
                annotationRepositoryService.GetAnnotationsOfType(typeof(ToStringAttribute), false, false);
            while (enumerator.MoveNext())
            {
                // Iterates over declarations to which our attribute has been applied. If the attribute weren't
                // a MulticastAttribute, that would be just the declarations that it annotates. With multicasting, it 
                // can be far more declarations.

                MetadataDeclaration targetDeclaration = enumerator.Current.TargetElement;

                // Multicasting ensures that our attribute is only applied to methods, so there is little chance of 
                // a class cast error here:
                MethodDefDeclaration targetMethod = (MethodDefDeclaration) targetDeclaration;

                AddHelloWorldToMethod(targetMethod, consoleWriteLine);
            }

            return true;
        }

        private IMethod FindConsoleWriteLine()
        {
            // Represents the module (= assembly) that we're modifying:
            ModuleDeclaration module = this.Project.Module;

            // Finds the System.Console type usable in that module. We don't know exactly where it comes from. It could
            // be mscorlib in .NET Framework or something else in .NET Core:
            INamedType console = (INamedType) module.FindType(typeof(Console));

            // Finds the one overload that we want: System.Console.WriteLine(System.String):
            IGenericMethodDefinition method = module.FindMethod( console, "WriteLine",
                declaration => declaration.Parameters.Count == 1 && 
                               declaration.Parameters[0].ParameterType.GetReflectionName() == "System.String" );

            return method;
        }

        private static void AddHelloWorldToMethod(MethodDefDeclaration targetMethod, IMethod consoleWriteLine)
        {
            // Removes the original code from the method body. Without this, you would get exceptions:
            InstructionBlock originalCode = targetMethod.MethodBody.RootInstructionBlock;
            originalCode.Detach();

            // Replaces the method body's content:
            InstructionBlock root = targetMethod.MethodBody.CreateInstructionBlock();
            targetMethod.MethodBody.RootInstructionBlock = root;

            InstructionBlock helloWorldBlock = root.AddChildBlock();
            InstructionSequence helloWorldSequence = helloWorldBlock.AddInstructionSequence();
            using (var writer = InstructionWriter.GetInstance())
            {
                // Add instructions to the beginning of the method body:
                writer.AttachInstructionSequence(helloWorldSequence);

                // Say that what follows is compiler-generated code:
                writer.EmitSymbolSequencePoint(SymbolSequencePoint.Hidden);

                // Emit a call to Console.WriteLine("Hello, world!"):
                writer.EmitInstructionString(OpCodeNumber.Ldstr, "Hello, world!");
                writer.EmitInstructionMethod(OpCodeNumber.Call, consoleWriteLine);

                writer.DetachInstructionSequence();
            }

            // Re-adding the original code at the end:
            root.AddChildBlock(originalCode);
        }
    }
}