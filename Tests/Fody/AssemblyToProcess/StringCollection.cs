﻿using System.Collections.Generic;

 namespace PostSharp.Community.ToString.Tests.Fody.AssemblyToProcess
 {
     [ToString]
     public class StringCollection
     {
         public int Count { get; set; }

         public IEnumerable<string> Collection { get; set; }
     }
 }