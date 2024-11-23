import * as path from 'path';
import { FIGFont } from './FIGLet/FIGFont.js';
import { SmushingRules } from './FIGLet/SmushingRules.js';

/**
 * Factory class for creating FIGFontInfo instances
 */
export class FIGFontInfo {
    private static _default: FIGFontInfo | null = null;
    private _name: string | null = null;
    private _filePath: string | null = null;
    private _font: FIGFont;

    /**
     * Gets the default FIGFontInfo instance.
     */
    public static async getDefault(): Promise<FIGFontInfo> {
        if (!FIGFontInfo._default) {
            FIGFontInfo._default = new FIGFontInfo(await FIGFont.getDefault());
        }
        if (!FIGFontInfo._default) {
            throw new Error("Default FIGfont not found");
        }
        return FIGFontInfo._default;
    }

    /**
     * Creates a new instance of FIGFontInfo from a file path.
     * @param filePath The path to the font file
     */
    static async fromFile(filePath: string): Promise<FIGFontInfo> {
        if (!filePath || filePath.trim() === '') {
            throw new Error('Value cannot be null or whitespace.');
        }
        
        const font = await FIGFont.fromFile(filePath);
        const instance = new FIGFontInfo(font);
        instance._filePath = filePath;
        instance._name = path.parse(filePath).name;
        return instance;
    }

    /**
     * Private constructor - use static factory methods to create instances
     */
    private constructor(font: FIGFont, name?: string) {
        if (!font) {
            throw new Error('Font cannot be null');
        }
        this._font = font;
        this._name = name || '';
    }

    /**
     * Gets the name of the font.
     */
    get name(): string {
        return this._name || (this._filePath ? path.parse(this._filePath).name : '');
    }

    /**
     * Gets the height of the font.
     */
    get height(): number {
        return this._font.height;
    }

    /**
     * Gets the baseline of the font.
     */
    get baseline(): number {
        return this._font.baseline;
    }

    /**
     * Gets the maximum length of the font.
     */
    get maxLength(): number {
        return this._font.maxLength;
    }

    /**
     * Gets the smushing rules of the font.
     */
    get smushingRules(): SmushingRules {
        return this._font.smushingRules;
    }

    /**
     * Gets the file path of the font.
     */
    get filePath(): string | null {
        return this._filePath;
    }

    /**
     * Gets the FIGFont instance.
     */
    get font(): FIGFont {
        return this._font;
    }
}