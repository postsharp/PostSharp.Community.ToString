namespace PostSharp.Community.ToString
{
    /// <summary>
    /// Specifies how to write the names of field and properties to the ToString output.
    /// </summary>
    public enum NamingConvention
    {
        /// <summary>
        /// Default. Write the name as is (e.g. write "ControllerKind" as "ControllerKind").
        /// </summary>
        Default,
        /// <summary>
        /// Convert name to camel case (e.g. write "ControllerKind" as "controllerKind").
        /// </summary>
        CamelCase
    }
}