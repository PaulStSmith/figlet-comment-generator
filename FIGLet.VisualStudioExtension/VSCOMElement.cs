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
     * __   _____  ___ ___  __  __ ___ _                   _   
     * \ \ / / __|/ __/ _ \|  \/  | __| |___ _ __  ___ _ _| |_ 
     *  \ V /\__ \ (_| (_) | |\/| | _|| / -_) '  \/ -_) ' \  _|
     *   \_/ |___/\___\___/|_|  |_|___|_\___|_|_|_\___|_||_\__|
     *                                                         
     */
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
