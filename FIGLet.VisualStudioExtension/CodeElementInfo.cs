using EnvDTE;

namespace FIGLet.VisualStudioExtension;

/*
 *   ___         _     ___ _                   _   ___       __     
 *  / __|___  __| |___| __| |___ _ __  ___ _ _| |_|_ _|_ _  / _|___ 
 * | (__/ _ \/ _` / -_) _|| / -_) '  \/ -_) ' \  _|| || ' \|  _/ _ \
 *  \___\___/\__,_\___|___|_\___|_|_|_\___|_||_\__|___|_||_|_| \___/
 *                                                                  
 */
/// <summary>
/// Represents information about a code element.
/// </summary>
public struct CodeElementInfo
{
    /// <summary>
    /// Gets the class name of the code element.
    /// </summary>
    public string ClassName { get; }

    /// <summary>
    /// Gets the method name of the code element.
    /// </summary>
    public string MethodName { get; }

    /// <summary>
    /// Gets the full name of the code element.
    /// </summary>
    public string FullName { get; }

    /// <summary>
    /// Gets or sets the code element.
    /// </summary>
    public CodeElement CodeElement { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeElementInfo"/> struct.
    /// </summary>
    /// <param name="className">The class name.</param>
    /// <param name="methodName">The method name.</param>
    /// <param name="fullName">The full name.</param>
    /// <param name="codeElement">The code element.</param>
    internal CodeElementInfo(string className, string methodName, string fullName, CodeElement codeElement)
    {
        ClassName = className;
        MethodName = methodName;
        FullName = fullName;
        CodeElement = codeElement;
    }

    /// <summary>
    /// Gets an empty <see cref="CodeElementInfo"/> instance.
    /// </summary>
    public static CodeElementInfo Empty => new(null, null, null, null);

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
    {
        return $"{ClassName}.{MethodName}";
    }
}
