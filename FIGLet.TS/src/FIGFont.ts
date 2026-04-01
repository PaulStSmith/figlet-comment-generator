import { SmushingRules } from './SmushingRules.js';
import { fileURLToPath } from 'url';
import { dirname, join } from 'path';
import { promises as fs } from 'fs';

/**
 * Represents a FIGfont used for rendering text in FIGLet style.
 */
export class FIGFont {
    private static _default: FIGFont | null = null;

    /**
     * Gets the default FIGfont.
     */
    public static async getDefault(): Promise<FIGFont> {
        if (!FIGFont._default) {
            FIGFont._default = await FIGFont.loadDefaultFont();
        }
        if (!FIGFont._default) {
            throw new Error("Default FIGfont not found");
        }
        return FIGFont._default;
    }

    /**
     * Gets the signature of the FIGfont.
     */
    public signature: string = "flf2a";

    /**
     * Gets the hard blank character used in the FIGfont.
     */
    public hardBlank: string = "#";

    /**
     * Gets the height of the FIGfont characters.
     */
    public height: number = 0;

    /**
     * Gets the baseline of the FIGfont characters.
     */
    public baseline: number = 0;

    /**
     * Gets the maximum length of the FIGfont characters.
     */
    public maxLength: number = 0;

    /**
     * Gets the old layout mode of the FIGfont.
     */
    public oldLayout: number = 0;

    /**
     * Gets the print direction of the FIGfont.
     */
    public printDirection: number = 0;

    /**
     * Gets the full layout mode of the FIGfont.
     */
    public fullLayout: number = 0;

    /**
     * Gets the dictionary of characters in the FIGfont.
     */
    public characters: Map<string, string[]> = new Map();

    /**
     * Gets the smushing rules for the FIGfont.
     */
    public smushingRules: SmushingRules = SmushingRules.None;

    /**
     * Gets the comments for the FIGfont.
     */
    public comments: string = "";

    /**
     * Creates a FIGFont from a file.
     */
    public static async fromFile(path: string | null): Promise<FIGFont | null> {
        if (!path) {
            return null;
        }

        try {
            const text = await fs.readFile(path, 'utf-8');
            return FIGFont.fromText(text);
        } catch {
            return null;
        }
    }

    /**
     * Creates a FIGFont from text content.
     */
    public static fromText(text: string | null): FIGFont | null {
        if (!text) {
            return null;
        }
        return FIGFont.fromLines(text.split('\n'));
    }

    /**
     * Creates a FIGFont from an array of lines.
     */
    public static fromLines(lines: string[] | null): FIGFont | null {
        if (!lines || lines.length === 0) {
            return null;
        }

        const font = new FIGFont();

        // Parse header
        const header = lines[0].trim();
        if (!header.startsWith("flf2a")) {
            throw new Error("Invalid FIGfont format");
        }

        const headerParts = header.split(' ');
        font.signature = headerParts[0];
        font.hardBlank = headerParts[0].substring(5, 6);
        font.height = parseInt(headerParts[1]);
        font.baseline = parseInt(headerParts[2]);
        font.maxLength = parseInt(headerParts[3]);
        font.oldLayout = parseInt(headerParts[4]);
        const commentLines = parseInt(headerParts[5]);
        
        if (headerParts.length > 6) {
            font.printDirection = parseInt(headerParts[6]);
        }
        if (headerParts.length > 7) {
            font.fullLayout = parseInt(headerParts[7]);
        }

        font.comments = lines.slice(1, 1 + commentLines).join('\n');

        // Skip header and comments
        let currentLine = 1 + commentLines;

        // Load required characters (ASCII 32-126)
        for (let charCode = 32; charCode <= 126; charCode++) {
            const charLines: string[] = new Array(font.height);
            for (let i = 0; i < font.height; i++) {
                charLines[i] = lines[currentLine + i]
                    .replace(/[@\n\r]+$/, '')  // TrimEnd equivalent
                    // Special case for different hard blank character
                    .replace(/#$/, font.hardBlank === "#" ? "#" : "");
            }
            font.characters.set(String.fromCharCode(charCode), charLines);
            currentLine += font.height;
        }

        // Continue reading additional characters if they exist
        while (currentLine + font.height <= lines.length) {
            const codeLine = lines[currentLine];
            if (!codeLine || !/^\d/.test(codeLine)) {
                break;
            }

            const codePoint = parseInt(codeLine.split(' ')[0]);
            currentLine++; // Move past the code point line

            const charLines: string[] = new Array(font.height);
            for (let i = 0; i < font.height; i++) {
                if (currentLine + i >= lines.length) break;
                charLines[i] = lines[currentLine + i].replace(/[@\n\r]+$/, '');
            }
            font.characters.set(String.fromCharCode(codePoint), charLines);
            currentLine += font.height;
        }

        // Parse the layout parameters
        FIGFont.parseLayoutParameters(font);

        return font;
    }

    /**
     * Loads the default FIGfont.
     */
    private static async loadDefaultFont(): Promise<FIGFont | null> {
        try {
            // Get the current module's directory
            const currentFileUrl = import.meta.url;
            const currentDir = dirname(fileURLToPath(currentFileUrl));
            
            // Construct path to the font file relative to the current module
            const fontPath = join(currentDir, '..', 'assets', 'fonts', 'small.flf');
            
            // Read the font file directly using fs
            const fontContent = await fs.readFile(fontPath, 'utf-8');
            return FIGFont.fromText(fontContent);
        } catch (error) {
            console.error('Error loading default font:', error);
            return null;
        }
    }

    /**
     * Parses the layout parameters to determine smushing rules for the FIGfont.
     */
    private static parseLayoutParameters(font: FIGFont): void {
        // First, determine if we should use full_layout or old_layout
        let layoutMask: number;

        if (font.fullLayout > 0) {
            // Full layout is present
            layoutMask = font.fullLayout;

            // Check if horizontal smushing is enabled
            const horizontalSmushingEnabled = (layoutMask & 1) === 1;
            if (!horizontalSmushingEnabled) {
                font.smushingRules = SmushingRules.None;
                return;
            }

            // Extract just the rules part (bits 1-6)
            layoutMask = (layoutMask >> 1) & 0x3F;
        } else {
            // Use old layout
            layoutMask = font.oldLayout;

            // In old layout, -1 means no smushing
            if (layoutMask === -1) {
                font.smushingRules = SmushingRules.None;
                return;
            }

            // In old layout, 0 means kerning
            if (layoutMask === 0) {
                font.smushingRules = SmushingRules.None;
                return;
            }

            // For positive values, extract the rules
            if (layoutMask > 0) {
                // Convert old layout to new layout rules format
                layoutMask &= 0x3F;
            }
        }

        // Apply the final smushing rules
        font.smushingRules = layoutMask;
    }

    /**
     * Determines if the FIGfont has a specific smushing rule.
     */
    public hasSmushingRule(rule: SmushingRules): boolean {
        return (this.smushingRules & rule) === rule;
    }
}