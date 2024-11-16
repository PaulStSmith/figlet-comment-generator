using EnvDTE;

namespace FIGLet.VisualStudioExtension;

internal partial class CodeElementDetector
{
    /// <summary>
    /// Represents member elements in Visual Studio.
    /// </summary>
    public enum VSMemberElement
    {
        /// <summary>
        /// Represents a function element.
        /// </summary>
        Function = vsCMElement.vsCMElementFunction,

        /// <summary>
        /// Represents a variable element.
        /// </summary>
        Variable = vsCMElement.vsCMElementVariable,

        /// <summary>
        /// Represents a property element.
        /// </summary>
        Property = vsCMElement.vsCMElementProperty,

        /// <summary>
        /// Represents a parameter element.
        /// </summary>
        Parameter = vsCMElement.vsCMElementParameter,

        /// <summary>
        /// Represents an attribute element.
        /// </summary>
        Attribute = vsCMElement.vsCMElementAttribute,

        /// <summary>
        /// Represents an event element.
        /// </summary>
        Event = vsCMElement.vsCMElementEvent,

        /// <summary>
        /// Represents a delegate element.
        /// </summary>
        Delegate = vsCMElement.vsCMElementDelegate
    }
}
