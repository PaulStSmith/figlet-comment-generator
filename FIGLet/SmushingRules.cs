namespace ByteForge.FIGLet;

/*
 *  ___              _    _           ___      _        
 * / __|_ __ _  _ __| |_ (_)_ _  __ _| _ \_  _| |___ ___
 * \__ \ '  \ || (_-< ' \| | ' \/ _` |   / || | / -_)_-<
 * |___/_|_|_\_,_/__/_||_|_|_||_\__, |_|_\\_,_|_\___/__/
 *                              |___/                   
 */

/// <summary>
/// Specifies the rules for smushing characters together in FIGLet text.
/// </summary>
[Flags]
public enum SmushingRules
{
    /// <summary>
    /// No smushing rules applied.
    /// </summary>
    None = 0,

    /// <summary>
    /// Smushes characters that are the same.
    /// </summary>
    EqualCharacter = 1,

    /// <summary>
    /// Smushes underscores with other characters.
    /// </summary>
    Underscore = 2,

    /// <summary>
    /// Smushes characters based on a hierarchy of character importance.
    /// </summary>
    Hierarchy = 4,

    /// <summary>
    /// Smushes characters that form opposite pairs.
    /// </summary>
    OppositePair = 8,

    /// <summary>
    /// Smushes characters to form a large 'X' shape.
    /// </summary>
    BigX = 16,

    /// <summary>
    /// Smushes hard blank characters.
    /// </summary>
    HardBlank = 32
}
