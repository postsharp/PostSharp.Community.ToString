using PostSharp.Sdk.CodeModel;
using PostSharp.Sdk.CodeModel.Collections;

namespace PostSharp.Community.ToString.Weaver
{
    public class Configuration
    {
        public string PropertiesSeparator { get; set; } = ",";
        public string NameValueSeparator { get; set; } = ":";
        public bool WrapWithBraces { get; set; } = true;
        public bool WriteTypeName { get; set; } = true;

        public static Configuration ReadConfiguration(IAnnotationValue value, Configuration baseConfiguration)
        {
            Configuration config = new Configuration();
            MemberValuePairCollection namedArguments = value.NamedArguments;
            config.WrapWithBraces = (bool)(namedArguments[nameof(ToStringAttribute.WrapWithBraces)]?.Value.Value ?? baseConfiguration.WrapWithBraces);
            config.WriteTypeName = (bool) (namedArguments[nameof(ToStringAttribute.WriteTypeName)]?.Value.Value ??
                                           baseConfiguration.WriteTypeName);
            config.NameValueSeparator = (string)(namedArguments[nameof(ToStringAttribute.PropertyNameToValueSeparator)]?.Value.Value ?? baseConfiguration.NameValueSeparator);
            config.PropertiesSeparator = (string)(namedArguments[nameof(ToStringAttribute.PropertiesSeparator)]?.Value.Value ?? baseConfiguration.PropertiesSeparator);
            return config;
        }
    }
}