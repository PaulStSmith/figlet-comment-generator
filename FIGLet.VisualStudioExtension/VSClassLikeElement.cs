using EnvDTE;

namespace FIGLet.VisualStudioExtension;

internal partial class CodeElementDetector
{
    /// <summary>
    /// Represents class-like elements in Visual Studio.
    /// </summary>
    public enum VSClassLikeElement
    {
        /// <summary>
        /// Represents a class element.
        /// </summary>
        Class = vsCMElement.vsCMElementClass,

        /// <summary>
        /// Represents an interface element.
        /// </summary>
        Interface = vsCMElement.vsCMElementInterface,

        /// <summary>
        /// Represents a struct element.
        /// </summary>
        Struct = vsCMElement.vsCMElementStruct,

        /// <summary>
        /// Represents a union element.
        /// </summary>
        Union = vsCMElement.vsCMElementUnion,

        /// <summary>
        /// Represents an enum element.
        /// </summary>
        Enum = vsCMElement.vsCMElementEnum,

        /// <summary>
        /// Represents a module element.
        /// </summary>
        Module = vsCMElement.vsCMElementModule,

        /// <summary>
        /// Represents a UDT declaration element.
        /// </summary>
        UDTDecl = vsCMElement.vsCMElementUDTDecl
    }
}
