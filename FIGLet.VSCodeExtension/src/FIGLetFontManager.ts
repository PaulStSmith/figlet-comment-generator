import * as fs from 'fs';
import * as path from 'path';
import { FIGFontInfo } from './FIGFontInfo';
import { promisify } from 'util';

const readdir = promisify(fs.readdir);
const exists = promisify(fs.exists);

/**
 * Manages FIGlet fonts by loading them from a directory and checking their existence.
 */
export class FIGLetFontManager {
    private static _fontDirectory: string | null = null;
    private static _availableFonts: ReadonlyArray<FIGFontInfo> = [];

    /**
     * Gets the list of available fonts.
     */
    public static get availableFonts(): ReadonlyArray<FIGFontInfo> {
        if(!this._availableFonts) {
            throw new Error('No fonts available. Call setFontDirectory() first.');
        }
        return this._availableFonts;
    }

    /**
     * Loads FIGlet fonts from the specified directory.
     * @param directory The directory to load fonts from
     */
    private static async loadFontsFromDirectory(directory: string | null): Promise<void> {
        var defaultFont = await FIGFontInfo.getDefault();
        const fontList: FIGFontInfo[] = [defaultFont];

        try {
            if (!directory) {
                return;
            }

            const directoryExists = await exists(directory);
            if (!directoryExists) {
                throw new Error(`Font directory not found: ${directory}`);
            }

            const files = await readdir(directory);
            for (const file of files) {
                if (path.extname(file).toLowerCase() === '.flf') {
                    try {
                        const fontInfo = await FIGFontInfo.fromFile(path.join(directory, file));
                        fontList.push(fontInfo);
                    } catch (error) {
                        console.error(`Error loading font ${file}: ${error}`);
                    }
                }
            }
        } finally {
            this._availableFonts = Object.freeze(fontList);
        }
    }

    /**
     * Sets the directory from which to load FIGlet fonts.
     * @param directory The directory to load fonts from
     */
    static async setFontDirectory(directory: string | null): Promise<void> {
        if (this._fontDirectory !== directory) {
            this._fontDirectory = directory;
            await this.loadFontsFromDirectory(directory);
        }
    }

    /**
     * Gets a font by its name.
     * @param name The name of the font to get
     * @returns The font info, or null if not found
     */
    static getFontByName(name: string): FIGFontInfo | null {
        return this._availableFonts.find(f => f.name === name) || null;
    }

    /**
     * Gets the current font directory.
     */
    static get fontDirectory(): string | null {
        return this._fontDirectory;
    }
}