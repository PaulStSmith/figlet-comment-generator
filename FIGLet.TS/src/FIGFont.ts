import { SmushingRules } from './SmushingRules.js';
import { fileURLToPath } from 'url';
import { dirname, join } from 'path';
import { promises as fs } from 'fs';
import { inflateRaw } from 'zlib';
import { promisify } from 'util';

const inflateRawAsync = promisify(inflateRaw);

/*
 *   ___ ___ ___ ___        _   
 *  | __|_ _/ __| __|__ _ _| |_ 
 *  | _| | | (_ | _/ _ \ ' \  _|
 *  |_| |___\___|_|\___/_||_\__|
 *                              
 */
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
     * Automatically detects ZIP archives by checking for the PK magic bytes
     * and extracts the first entry, matching the C# FIGFontStream behaviour.
     */
    public static async fromFile(path: string | null): Promise<FIGFont | null> {
        if (!path) {
            return null;
        }

        try {
            const buffer = await fs.readFile(path);

            // Detect ZIP by magic bytes 'PK' (0x50 0x4B)
            if (buffer.length >= 2 && buffer[0] === 0x50 && buffer[1] === 0x4B) {
                const text = await FIGFont.extractFirstZipEntry(buffer);
                return FIGFont.fromText(text);
            }

            return FIGFont.fromText(buffer.toString('utf-8'));
        } catch {
            return null;
        }
    }

    /**
     * Extracts and decodes the first entry from a ZIP archive buffer.
     * Supports Store (method 0) and Deflate (method 8) compression.
     * Throws if the buffer is not a valid local-file ZIP entry.
     */
    private static async extractFirstZipEntry(buffer: Buffer): Promise<string> {
        // Full local file header signature: PK\x03\x04
        if (buffer.length < 30 || buffer[2] !== 0x03 || buffer[3] !== 0x04) {
            throw new Error('Invalid ZIP format: missing local file header');
        }

        // ZIP local file header layout (all values little-endian):
        //   offset  2 — version needed        (2 bytes)
        //   offset  6 — general purpose flags (2 bytes)
        //   offset  8 — compression method    (2 bytes)  0=store, 8=deflate
        //   offset 14 — CRC-32               (4 bytes)
        //   offset 18 — compressed size       (4 bytes)
        //   offset 22 — uncompressed size     (4 bytes)
        //   offset 26 — filename length       (2 bytes)
        //   offset 28 — extra field length    (2 bytes)
        //   offset 30 — filename + extra + data
        const compressionMethod = buffer.readUInt16LE(8);
        const compressedSize    = buffer.readUInt32LE(18);
        const filenameLen       = buffer.readUInt16LE(26);
        const extraLen          = buffer.readUInt16LE(28);
        const dataOffset        = 30 + filenameLen + extraLen;

        const compressedData = buffer.subarray(dataOffset, dataOffset + compressedSize);

        if (compressionMethod === 0) {
            // Store — data is uncompressed
            return compressedData.toString('latin1');
        }

        if (compressionMethod === 8) {
            // Deflate
            const decompressed = await inflateRawAsync(compressedData);
            return decompressed.toString('latin1');
        }

        throw new Error(`Unsupported ZIP compression method: ${compressionMethod}`);
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

        if (isNaN(font.height) || isNaN(font.baseline) || isNaN(font.maxLength) || isNaN(font.oldLayout) || isNaN(commentLines)) {
            throw new Error("Invalid FIGfont header parameters");
        }

        if (lines.length < 1 + commentLines + font.height * (126 - 32 + 1)) {
            throw new Error("Not enough lines in FIGfont file for required characters");
        }

        // Skip header and comments
        let currentLine = 1 + commentLines;

        // Load required characters (ASCII 32-126)
        for (let charCode = 32; charCode <= 126; charCode++) {
            const endmark = FIGFont.detectEndmark(lines, currentLine);
            const endmarkRe = new RegExp(`[${endmark.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}\\n\\r]+$`);
            const charLines: string[] = new Array(font.height);
            for (let i = 0; i < font.height; i++) {
                charLines[i] = lines[currentLine + i]
                    .replace(endmarkRe, '')
                    // Special case for different hard blank character
                    .replace(/#$/, font.hardBlank === "#" ? "#" : "");
            }
            font.characters.set(String.fromCodePoint(charCode), charLines);
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

            const endmark = FIGFont.detectEndmark(lines, currentLine);
            const endmarkRe = new RegExp(`[${endmark.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}\\n\\r]+$`);
            const charLines: string[] = new Array(font.height);
            for (let i = 0; i < font.height; i++) {
                if (currentLine + i >= lines.length) break;
                charLines[i] = lines[currentLine + i].replace(endmarkRe, '');
            }
            font.characters.set(String.fromCodePoint(codePoint), charLines);
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
            const fontPath = join(currentDir, 'assets', 'fonts', 'small.flf');
            
            // Read the font file directly using fs
            const fontContent = await fs.readFile(fontPath, 'utf-8');
            return FIGFont.fromText(fontContent);
        } catch (error) {
            console.error('Error loading default font:', error);
            return null;
        }
    }

    /**
     * Detects the endmark character used by a glyph block from the last character
     * of its first raw line (after stripping CR/LF).  Falls back to '@' if the
     * line is empty.
     */
    private static detectEndmark(lines: string[], blockStart: number): string {
        if (blockStart >= lines.length) return '@';
        const firstRaw = lines[blockStart].replace(/[\r\n]+$/, '');
        return firstRaw.length > 0 ? firstRaw[firstRaw.length - 1] : '@';
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