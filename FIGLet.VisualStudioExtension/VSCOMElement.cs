using EnvDTE;

namespace FIGLet.VisualStudioExtension;

internal partial class CodeElementDetector
{
    /// <summary>
    /// Represents COM elements in Visual Studio.
    /// </summary>
    public enum VSCOMElement
    {
        /// <summary>
        /// Represents a map element.
        /// </summary>
        Map = vsCMElement.vsCMElementMap,

        /// <summary>
        /// Represents a map entry element.
        /// </summary>
        MapEntry = vsCMElement.vsCMElementMapEntry,

        /// <summary>
        /// Represents an IDL import element.
        /// </summary>
        IDLImport = vsCMElement.vsCMElementIDLImport,

        /// <summary>
        /// Represents an IDL import library element.
        /// </summary>
        IDLImportLib = vsCMElement.vsCMElementIDLImportLib,

        /// <summary>
        /// Represents an IDL co-class element.
        /// </summary>
        IDLCoClass = vsCMElement.vsCMElementIDLCoClass,

        /// <summary>
        /// Represents an IDL library element.
        /// </summary>
        IDLLibrary = vsCMElement.vsCMElementIDLLibrary
    }
}
