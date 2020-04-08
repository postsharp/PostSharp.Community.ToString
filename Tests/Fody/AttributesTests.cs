using PostSharp.Community.ToString.Tests.Fody.AssemblyToProcess;
using Xunit;

namespace PostSharp.Community.ToString.Tests.Fody
{
    public class AttributesTests 
    {
        string PropertyNameToValueSeparator = "==";
        string PropertiesSeparator = "==";
    
        [Fact]
        public void NormalClassTest_ShouldUseCustomPropertyNameToValueSeparator()
        {
            var instance = new NormalClassValueSeparator();
            instance.X = 1;
            instance.Y = "2";
            instance.Z = 4.5;
            instance.V = 'C';

            var result = instance.ToString();

            Assert.Equal(
                string.Format("{{T{0}\"NormalClass\", X{0}1, Y{0}\"2\", Z{0}4.5, V{0}\"C\"}}", PropertyNameToValueSeparator),
                result);
        }

        [Fact]
        public void NormalClassTest_ShouldUseCustomPropertiesSeparator()
        {
            var instance = new NormalClassPropertiesSeparator();
            instance.X = 1;
            instance.Y = "2";
            instance.Z = 4.5;
            instance.V = 'C';

            var result = instance.ToString();

            Assert.Equal(
                string.Format("{{T: \"NormalClass\"{0}X: 1{0}Y: \"2\"{0}Z: 4.5{0}V: \"C\"}}", PropertiesSeparator),
                result);
        }

        [Fact]
        public void NormalClassTest_ShouldNotWrapInBrackets()
        {
            var instance = new NormalClassNoWrap();
            instance.X = 1;
            instance.Y = "2";
            instance.Z = 4.5;
            instance.V = 'C';

            var result = instance.ToString();

            Assert.Equal(
                "T: \"NormalClass\", X: 1, Y: \"2\", Z: 4.5, V: \"C\"",
                result);
        }

        [Fact]
        public void NormalClassTest_ShouldNotWriteClassName()
        {
            var instance = new NormalClassNoTypeName();
            instance.X = 1;
            instance.Y = "2";
            instance.Z = 4.5;
            instance.V = 'C';

            var result = instance.ToString();

            Assert.Equal(
                "{X: 1, Y: \"2\", Z: 4.5, V: \"C\"}",
                result);
        }
    }
}