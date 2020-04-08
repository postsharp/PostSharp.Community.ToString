﻿using System;

 namespace PostSharp.Community.ToString.Tests.Fody.AssemblyToProcess
 {
     [ToString]
     public class GuidClass
     {
         public int X { get; set; }

         public Guid Y { get; set; }
     }
 }