/*
 *   ___              _    _           ___      _        
 *  / __|_ __ _  _ __| |_ (_)_ _  __ _| _ \_  _| |___ ___
 *  \__ \ '  \ || (_-< ' \| | ' \/ _` |   / || | / -_)_-<
 *  |___/_|_|_\_,_/__/_||_|_|_||_\__, |_|_\\_,_|_\___/__/
 *                               |___/                   
 */
/**
 * Specifies the rules for smushing characters together in FIGLet text.
 */
export enum SmushingRules {
    /**
     * No smushing rules applied.
     */
    None = 0,

    /**
     * Smushes characters that are the same.
     */
    EqualCharacter = 1,

    /**
     * Smushes underscores with other characters.
     */
    Underscore = 2,

    /**
     * Smushes characters based on a hierarchy of character importance.
     */
    Hierarchy = 4,

    /**
     * Smushes characters that form opposite pairs.
     */
    OppositePair = 8,

    /**
     * Smushes characters to form a large 'X' shape.
     */
    BigX = 16,

    /**
     * Smushes hard blank characters.
     */
    HardBlank = 32
}