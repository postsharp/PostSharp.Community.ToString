using System;
using PostSharp.Sdk.CodeModel;

namespace PostSharp.Community.ToString.Weaver
{
    public static class ILHelpers
    {
        /// <summary>
        /// Emits:
        /// <code>
        ///   brfalse elseBranch;
        ///   thenStatement();
        ///   br end;
        /// elseBranch:
        ///   elseStatement();
        /// end: 
        /// </code>
        /// </summary>
        public static void IfNotZero(this InstructionWriter writer,
            Action thenStatement,
            Action elseStatement)
        {
            InstructionSequence elseSequence =
                writer.CurrentInstructionSequence.ParentInstructionBlock.AddInstructionSequence();
            InstructionSequence endSequence =
                writer.CurrentInstructionSequence.ParentInstructionBlock.AddInstructionSequence();
          
            writer.EmitBranchingInstruction(OpCodeNumber.Brfalse, elseSequence);

            thenStatement();
            writer.EmitBranchingInstruction(OpCodeNumber.Br, endSequence);
            writer.DetachInstructionSequence();
            writer.AttachInstructionSequence(elseSequence);
            elseStatement();
            writer.EmitBranchingInstruction(OpCodeNumber.Br, endSequence);
            writer.DetachInstructionSequence();
            writer.AttachInstructionSequence(endSequence);
        }
    }
}