﻿using System.Collections.Generic;

 namespace PostSharp.Community.ToString.Tests.Fody.AssemblyToProcess
 {
     [ToString]
     public class ObjectCollection
     {
         public int Count { get; set; }

         public IEnumerable<NormalClass> Collection { get; set; }
     }
 }