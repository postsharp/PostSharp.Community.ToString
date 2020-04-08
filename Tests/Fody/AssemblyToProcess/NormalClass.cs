﻿namespace PostSharp.Community.ToString.Tests.Fody.AssemblyToProcess
 {
     [ToString]
     public class NormalClass
     {
         public int X { get; set; }

         public string Y { get; set; }

         public double Z { get; set; }

         public char V { get; set; }
     }
     [ToString(WrapWithBraces = false)]
     public class NormalClassNoWrap
     {
         public int X { get; set; }

         public string Y { get; set; }

         public double Z { get; set; }

         public char V { get; set; }
     }
     [ToString(PropertiesSeparator = "==")]
     public class NormalClassPropertiesSeparator
     {
         public int X { get; set; }

         public string Y { get; set; }

         public double Z { get; set; }

         public char V { get; set; }
     }  
     [ToString(PropertyNameToValueSeparator = "==")]
     public class NormalClassValueSeparator
     {
         public int X { get; set; }

         public string Y { get; set; }

         public double Z { get; set; }

         public char V { get; set; }
     } 
     [ToString(WriteTypeName = false)]
     public class NormalClassNoTypeName
     {
         public int X { get; set; }

         public string Y { get; set; }

         public double Z { get; set; }

         public char V { get; set; }
     }
 }