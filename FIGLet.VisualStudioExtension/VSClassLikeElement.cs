using EnvDTE;

namespace FIGLet.VisualStudioExtension;

/*
 *   ___         _     ___ _                   _   ___      _          _           
 *  / __|___  __| |___| __| |___ _ __  ___ _ _| |_|   \ ___| |_ ___ __| |_ ___ _ _ 
 * | (__/ _ \/ _` / -_) _|| / -_) '  \/ -_) ' \  _| |) / -_)  _/ -_) _|  _/ _ \ '_|
 *  \___\___/\__,_\___|___|_\___|_|_|_\___|_||_\__|___/\___|\__\___\__|\__\___/_|  
 *                                                                                 
 */
internal partial class CodeElementDetector
{
    /*
     * __   _____  ___ _            _    _ _       ___ _                   _   
     * \ \ / / __|/ __| |__ _ _____| |  (_) |_____| __| |___ _ __  ___ _ _| |_ 
     *  \ V /\__ \ (__| / _` (_-<_-< |__| | / / -_) _|| / -_) '  \/ -_) ' \  _|
     *   \_/ |___/\___|_\__,_/__/__/____|_|_\_\___|___|_\___|_|_|_\___|_||_\__|
     *                                                                         
     */
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
