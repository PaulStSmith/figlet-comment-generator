/*
 *   _                       _   __  __         _     
 *  | |   __ _ _  _ ___ _  _| |_|  \/  |___  __| |___ 
 *  | |__/ _` | || / _ \ || |  _| |\/| / _ \/ _` / -_)
 *  |____\__,_|\_, \___/\_,_|\__|_|  |_\___/\__,_\___|
 *             |__/                                   
 */
/**
 * Specifies the layout mode for rendering FIGLet text.
 */
export enum LayoutMode {
    /**
     * Full size layout mode.
     */
    FullSize = -1,

    /**
     * Kerning layout mode.
     */
    Kerning = 0,

    /**
     * Smushing layout mode.
     */
    Smushing = 1,

    /**
     * The default layout mode, which is smushing.
     */
    Default = Smushing
}