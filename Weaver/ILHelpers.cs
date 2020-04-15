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
        
        /// <summary>
        /// Emits:
        /// <code>
        ///   br begin;
        /// begin:
        ///   condition();
        ///   brfalse end;
        ///   body();
        ///   br begin;
        /// end:
        /// </code> 
        /// </summary>
        public static void WhileNotZero(this InstructionWriter writer,
            Action condition,
            Action body)
        {
            InstructionSequence loopBegin =
                writer.CurrentInstructionSequence.ParentInstructionBlock.AddInstructionSequence();
            InstructionSequence loopEnd =
                writer.CurrentInstructionSequence.ParentInstructionBlock.AddInstructionSequence();
            
            writer.EmitBranchingInstruction(OpCodeNumber.Br, loopBegin);
            writer.DetachInstructionSequence();
            writer.AttachInstructionSequence(loopBegin);

            condition();

            writer.EmitBranchingInstruction(OpCodeNumber.Brfalse, loopEnd);

            body();

            writer.EmitBranchingInstruction(OpCodeNumber.Br, loopBegin);
            writer.DetachInstructionSequence();
            writer.AttachInstructionSequence(loopEnd);
        }
    }
}