using EnvDTE;

namespace FIGLet.VisualStudioExtension;

internal partial class CodeElementDetector
{
    /// <summary>
    /// Represents statement elements in Visual Studio.
    /// </summary>
    public enum VSStatementElement
    {
        /// <summary>
        /// Represents a local declaration statement.
        /// </summary>
        LocalDecl = vsCMElement.vsCMElementLocalDeclStmt,

        /// <summary>
        /// Represents a function invocation statement.
        /// </summary>
        FunctionInvoke = vsCMElement.vsCMElementFunctionInvokeStmt,

        /// <summary>
        /// Represents a property set statement.
        /// </summary>
        PropertySet = vsCMElement.vsCMElementPropertySetStmt,

        /// <summary>
        /// Represents an assignment statement.
        /// </summary>
        Assignment = vsCMElement.vsCMElementAssignmentStmt,

        /// <summary>
        /// Represents an inherits statement.
        /// </summary>
        Inherits = vsCMElement.vsCMElementInheritsStmt,

        /// <summary>
        /// Represents an implements statement.
        /// </summary>
        Implements = vsCMElement.vsCMElementImplementsStmt,

        /// <summary>
        /// Represents an option statement.
        /// </summary>
        Option = vsCMElement.vsCMElementOptionStmt,

        /// <summary>
        /// Represents a VB attribute statement.
        /// </summary>
        VBAttribute = vsCMElement.vsCMElementVBAttributeStmt,

        /// <summary>
        /// Represents a declare declaration.
        /// </summary>
        Declare = vsCMElement.vsCMElementDeclareDecl,

        /// <summary>
        /// Represents a define statement.
        /// </summary>
        Define = vsCMElement.vsCMElementDefineStmt,

        /// <summary>
        /// Represents an include statement.
        /// </summary>
        Include = vsCMElement.vsCMElementIncludeStmt,

        /// <summary>
        /// Represents a using statement.
        /// </summary>
        Using = vsCMElement.vsCMElementUsingStmt,

        /// <summary>
        /// Represents an import statement.
        /// </summary>
        Import = vsCMElement.vsCMElementImportStmt
    }
}
