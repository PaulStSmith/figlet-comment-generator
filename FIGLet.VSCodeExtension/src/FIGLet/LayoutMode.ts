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
    Default = -2
}