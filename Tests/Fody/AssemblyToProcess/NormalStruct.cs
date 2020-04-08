﻿namespace PostSharp.Community.ToString.Tests.Fody.AssemblyToProcess
 {
     [ToString]
     public struct NormalStruct
     {
         public int X { get; set; }

         public string Y { get; set; }

         public double Z { get; set; }
     }
 }