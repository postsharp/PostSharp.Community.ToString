﻿using System.Collections.Generic;

 namespace PostSharp.Community.ToString.Tests.Fody.AssemblyToProcess
 {
     [ToString]
     public class IntCollection
     {
         public int Count { get; set; }

         public IEnumerable<int> Collection { get; set; }
     }
 }