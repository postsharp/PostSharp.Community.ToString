using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading;
using PostSharp.Community.ToString.Tests.Fody.AssemblyToProcess;
using Xunit;

namespace PostSharp.Community.ToString.Tests.Fody
{
    public class IntegrationTests 
    {
        public IntegrationTests()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        }
        [Fact]
        public void NormalClassTest()
        {
            var instance = new NormalClass();
            instance.X = 1;
            instance.Y = "2";
            instance.Z = 4.5;
            instance.V = 'C';

            var result  = instance.ToString();

            Assert.Equal("{NormalClass; X: 1, Y: 2, Z: 4.5, V: C}", result);
        }

        [Fact]
        public void NormalStructTest()
        {
            var instance = new NormalStruct();
            instance.X = 1;
            instance.Y = "2";
            instance.Z = 4.5;

            var result = instance.ToString();

            Assert.Equal("{NormalStruct; X: 1, Y: 2, Z: 4.5}", result);
        }

        [Fact]
        public void NestedClassTest()
        {
            var normalInstance = new NormalClass();
            normalInstance.X = 1;
            normalInstance.Y = "2";
            normalInstance.Z = 4.5;
            normalInstance.V = 'V';
            var nestedInstance = new NestedClass();
            nestedInstance.A = 10;
            nestedInstance.B = "11";
            nestedInstance.C = 12.25;
            nestedInstance.D = normalInstance;

            var result = nestedInstance.ToString();

            Assert.Equal("{NestedClass; A: 10, B: 11, C: 12.25, D: {NormalClass; X: 1, Y: 2, Z: 4.5, V: V}}", result);
        }

        [Fact]
        public void ClassWithIgnoredPropertiesTest()
        {
            var instance = new ClassWithIgnoredProperties();
            instance.Username = "user";
            instance.Password = "pass";
            instance.Age = 18;

            var result = instance.ToString();

            Assert.Equal("{ClassWithIgnoredProperties; Username: user, Age: 18}", result);
        }

        [Fact]
        public void NullTest()
        {
            var nestedInstance = new NestedClass();
            nestedInstance.A = 10;
            nestedInstance.B = "11";
            nestedInstance.C = 12.25;
            nestedInstance.D = null;

            var result = nestedInstance.ToString();

            Assert.Equal("{NestedClass; A: 10, B: 11, C: 12.25, D: null}", result);
        }

        [Fact]
        public void ClassWithParentInAnotherAssembly()
        {
            var instance = new Child();
            instance.InParent = 10;
            instance.InChild = 5;

            var result = instance.ToString();

            Assert.Equal("{Child; InChild: 5, InParent: 10}", result);
        }

        [Fact]
        public void ComplexClassWithParentInAnotherAssembly()
        {
            var instance = new ComplexChild();
            instance.InChildNumber = 1L;
            instance.InChildText = "2";
            instance.InChildCollection  = new[] {3};
            instance.InParentNumber = 4L;
            instance.InParentText = "5";
            instance.InParentCollection  = new[] {6};

            var result = instance.ToString();

            Assert.Equal("{ComplexChild; InChildNumber: 1, InChildText: 2, InChildCollection: [3], InParentNumber: 4, InParentText: 5, InParentCollection: [6]}", result);
        }

        [Fact]
        public void ClassWithGenericParentInAnotherAssembly()
        {
            var instance = new GenericChild();
            instance.InChild = "5";
            instance.GenericInParent = 6;

            var result = instance.ToString();

            Assert.Equal("{GenericChild; InChild: 5, GenericInParent: 6}", result);
        }

        [Fact]
        public void GuidErrorTest()
        {
            var instance = new ReferenceObject();
            instance.Id = Guid.Parse( "{f6ab1abe-5811-40e9-8154-35776d2e5106}" );
            instance.Name = "Test";

            var result = instance.ToString();

            Assert.Equal( "{ReferenceObject; Name: Test, Id: f6ab1abe-5811-40e9-8154-35776d2e5106}", result );
        }

        #region Collections

        [Fact]
        public void IntArray()
        {
            var nestedInstance = new IntCollection();
            nestedInstance.Collection = new[] { 1, 2, 3, 4, 5, 6 };
            nestedInstance.Count = 2;

            var result = nestedInstance.ToString();

            Assert.Equal("{IntCollection; Count: 2, Collection: [1,2,3,4,...]}", result);
        }

        [Fact]
        public void StringArray()
        {
            var nestedInstance = new StringCollection();
            nestedInstance.Collection = new List<string> { "foo", "bar" };
            nestedInstance.Count = 2;

            var result = nestedInstance.ToString();

            Assert.Equal("{StringCollection; Count: 2, Collection: [foo,bar]}", result);
        }

        [Fact]
        public void EmptyArray()
        {
            var nestedInstance = new IntCollection();
            nestedInstance.Collection = new int[] {};
            nestedInstance.Count = 0;

            var result = nestedInstance.ToString();

            Assert.Equal("{IntCollection; Count: 0, Collection: []}", result);
        }

        [Fact]
        public void NullArray()
        {
            var nestedInstance = new IntCollection();
            nestedInstance.Collection = null;
            nestedInstance.Count = 0;

            var result = nestedInstance.ToString();

            Assert.Equal("{IntCollection; Count: 0, Collection: null}", result);
        }

        [Fact]
        public void ObjectArray()
        {
            var arrayInstance = new ObjectCollection();
            arrayInstance.Count = 2;

            var instance = new NormalClass();
            instance.X = 1;
            instance.Y = "2";
            instance.Z = 4.5;
            instance.V = 'C';

            var array = new NormalClass[2];
            array[0] = instance;
            array[1] = null;

            arrayInstance.Collection = array;

            var result = arrayInstance.ToString();

            Assert.Equal("{ObjectCollection; Count: 2, Collection: [{NormalClass; X: 1, Y: 2, Z: 4.5, V: C},null]}", result);
        }

        [Fact]
        public void GenericClassWithCollection()
        {
            var instance = new GenericClass<GenericClassNormalClass>();
            instance.A = 1;

            var propInstance = new GenericClassNormalClass();
            propInstance.D = 2;
            propInstance.C = 3;

            var array = new GenericClassNormalClass[1];
            array[0] = propInstance;

            instance.B = array;

            var result = instance.ToString();

            Assert.Equal("{GenericClass; a: 1, A: 1, B: [{GenericClassNormalClass; D: 2, C: 3}]}", result);
        }

        [Fact]
        public void WithoutGenericParameter()
        {
            var instance = new WithoutGenericParameter();
            instance.Z = 12;
            instance.A = 1;
            var propInstance = new GenericClassNormalClass();
            propInstance.D = 3;
            propInstance.C = -4;
            var array = new GenericClassNormalClass[1];
            array[0] = propInstance;
            instance.B = array;

            var result = instance.ToString();

            Assert.Equal("{WithoutGenericParameter; a: 1, Z: 12, A: 1, B: [{GenericClassNormalClass; D: 3, C: -4}]}", result);
        }

        [Fact]
        public void WithGenericParameter()
        {
            var instance = new WithGenericParameter<GenericClassNormalClass>();
            instance.X = 12;
            instance.A = 1;
            var propInstance = new GenericClassNormalClass();
            propInstance.D = 3;
            propInstance.C = 4;
            var array = new GenericClassNormalClass[1];
            array[0] = propInstance;
            instance.B = array;

            var result = instance.ToString();

            Assert.Equal("{WithGenericParameter; a: 1, X: 12, A: 1, B: [{GenericClassNormalClass; D: 3, C: 4}]}", result);
        }

        [Fact]
        public void WithGenericProperty()
        {
            var instance = new WithPropertyOfGenericType<GenericClassNormalClass>();
            var propInstance = new GenericClassNormalClass();
            instance.GP = propInstance;
            propInstance.C = 1;
            propInstance.D = 3;

            var result = instance.ToString();

            Assert.Equal("{WithPropertyOfGenericType; GP: {GenericClassNormalClass; D: 3, C: 1}}", result);
        }

        [Fact]
        public void WithInheritedGenericProperty()
        {

            var instance = new WithInheritedPropertyOfGenericType();
            var propInstance = new GenericClassNormalClass();
            instance.GP = propInstance;
            propInstance.C = 1;
            propInstance.D = 3;
            instance.X = 6;

            var result = instance.ToString();

            Assert.Equal("{WithInheritedPropertyOfGenericType; X: 6, GP: {GenericClassNormalClass; D: 3, C: 1}}", result);
        }

        #endregion

        #region enums

        [Fact]
        public void EmptyEnum()
        {
            var instance = new EnumClass();

            var result = instance.ToString();

            Assert.Equal("{EnumClass; NormalEnum: A, FlagsEnum: G}", result);
        }

        [Fact]
        public void EnumWithValues()
        {
            var instance = new EnumClass(3,6);

            var result = instance.ToString();

            Assert.Equal("{EnumClass; NormalEnum: D, FlagsEnum: I, J}", result);
        }


        #endregion

        [Fact(Skip = "ISO time not implemented, maybe is not the best?")]
        public void TimeClassTest()
        {
            var instance = new TimeClass();
            instance.X = new DateTime(1988, 05, 23, 10, 30, 0, DateTimeKind.Utc);
            instance.Y = new TimeSpan(1, 2, 3, 4);

            var result = instance.ToString();

            Assert.Equal( $"{{TimeClass; X: {instance.X.ToString(CultureInfo.InvariantCulture)}, Y: {instance.Y.ToString()}}}", result );
        }

        [Fact]
        public void IndexerTest()
        {
            var instance = new ClassWithIndexer();
            instance.X = 1;
            instance.Y = 2;

            var result = instance.ToString();

            Assert.Equal("{ClassWithIndexer; X: 1, Y: 2}", result);
        }

        [Fact]
        public void KeepExistingToString()
        {
            var instance = new ClassWithToString();
            instance.X = 1;
            instance.Y = 2;

            var result = instance.ToString();

            Assert.Equal("XY", result);
        }

        [Fact]
        public void GuidClassTest()
        {
            var instance = new GuidClass();
            instance.X = 1;
            instance.Y = new Guid(1,2,3,4,5,6,7,8,9,10,11);

            var result = instance.ToString();

            Assert.Equal( "{GuidClass; X: 1, Y: 00000001-0002-0003-0405-060708090a0b}", result );
        }

        [Fact]
        public void ClassWithDerivedPropertiesTest()
        {
            var instance = new ClassWithDerivedProperties();
            var result = instance.ToString();

            Assert.Equal("{ClassWithDerivedProperties; NormalProperty: New, NormalProperty: Interface, VirtualProperty: Override Virtual, AbstractProperty: Override Abstract}", result);
        }
    }
}