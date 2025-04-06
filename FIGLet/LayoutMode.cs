namespace ByteForge.FIGLet;

/*
 *  _                       _   __  __         _     
 * | |   __ _ _  _ ___ _  _| |_|  \/  |___  __| |___ 
 * | |__/ _` | || / _ \ || |  _| |\/| / _ \/ _` / -_)
 * |____\__,_|\_, \___/\_,_|\__|_|  |_\___/\__,_\___|
 *            |__/                                   
 */

/// <summary>
/// Specifies the layout mode for rendering FIGLet text.
/// </summary>
public enum LayoutMode
{
    /// <summary>
    /// Full size layout mode.
    /// </summary>
    FullSize = -1,

    /// <summary>
    /// Kerning layout mode.
    /// </summary>
    Kerning = 0,

    /// <summary>
    /// Smushing layout mode.
    /// </summary>
    Smushing = 1,

    /// <summary>
    /// The default layout mode, which is smushing.
    /// </summary>
    Default = Smushing
}