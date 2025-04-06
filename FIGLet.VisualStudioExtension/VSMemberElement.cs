using EnvDTE;

namespace ByteForge.FIGLet.VisualStudioExtension;

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
     * __   _____ __  __           _             ___ _                   _   
     * \ \ / / __|  \/  |___ _ __ | |__  ___ _ _| __| |___ _ __  ___ _ _| |_ 
     *  \ V /\__ \ |\/| / -_) '  \| '_ \/ -_) '_| _|| / -_) '  \/ -_) ' \  _|
     *   \_/ |___/_|  |_\___|_|_|_|_.__/\___|_| |___|_\___|_|_|_\___|_||_\__|
     *                                                                       
     */
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
