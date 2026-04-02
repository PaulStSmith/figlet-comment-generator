import { FIGFont } from './FIGFont.js';
import { LayoutMode } from './LayoutMode.js';
import { SmushingRules } from './SmushingRules.js';

/**
 * Class for rendering text using FIGLet fonts.
 */
export class FIGLetRenderer {
    /**
     * Characters used for hierarchy smushing.
     */
    private static readonly HIERARCHY_CHARACTERS = "|/\\[]{}()<>";

    /**
     * ANSI color reset sequence appended to each output line when ANSI is enabled.
     */
    private static readonly ANSI_RESET = "\u001b[0m";

    /**
     * Dictionary of opposite character pairs for smushing.
     */
    private static readonly oppositePairs = new Map<string, string>([
        ['[', ']'], [']', '['],
        ['{', '}'], ['}', '{'],
        ['(', ')'], [')', '('],
        ['<', '>'], ['>', '<']
    ]);

    /**
     * Gets the FIGFont used for rendering text.
     */
    public readonly font: FIGFont;

    /**
     * Gets or sets the layout mode for rendering.
     */
    public layoutMode: LayoutMode;

    /**
     * Gets or sets the line separator used for rendering.
     */
    public lineSeparator: string;

    /**
     * Gets or sets a value indicating whether to process and preserve ANSI color codes.
     */
    public useANSIColors: boolean;

    /**
     * Gets or sets a value indicating whether to use paragraph mode for rendering.
     */
    public paragraphMode: boolean;

    /**
     * Renders the specified text using the given FIGFont and settings.
     * @param text The text to render.
     * @param font The FIGFont to use for rendering.
     * @param mode The layout mode to use for rendering.
     * @param lineSeparator The line separator to use.
     * @param useANSIColors Whether to process and preserve ANSI color codes.
     * @param paragraphMode Whether to use paragraph mode for rendering.
     * @returns The rendered text as a string.
     */
    public static render(
        text: string,
        font: FIGFont,
        mode: LayoutMode = LayoutMode.Default,
        lineSeparator: string = "\n",
        useANSIColors: boolean = false,
        paragraphMode: boolean = true
    ): string {
        if (!text) return "";
        const renderer = new FIGLetRenderer(font, mode, lineSeparator, useANSIColors, paragraphMode);
        return renderer.render(text);
    }

    /**
     * Initializes a new instance of the FIGLetRenderer class with the specified FIGFont and optional settings.
     * @param font The FIGFont to use for rendering.
     * @param mode The layout mode to use for rendering.
     * @param lineSeparator The line separator to use.
     * @param useANSIColors Whether to process and preserve ANSI color codes.
     * @param paragraphMode Whether to use paragraph mode for rendering.
     */
    constructor(
        font: FIGFont,
        mode: LayoutMode = LayoutMode.Default,
        lineSeparator: string = "\n",
        useANSIColors: boolean = false,
        paragraphMode: boolean = true
    ) {
        this.font = font;
        this.layoutMode = mode;
        this.lineSeparator = lineSeparator;
        this.useANSIColors = useANSIColors;
        this.paragraphMode = paragraphMode;
    }

    /**
     * Renders the specified text using the current FIGFont and settings.
     * @param text The text to render.
     * @returns The rendered text as a string.
     */
    public render(text: string): string {
        if (!text) return "";

        if (this.paragraphMode) {
            // Split on \r\n or \n; empty/whitespace-only segments produce blank rendered lines
            const emptyLine = this.lineSeparator.repeat(this.font.height);
            const paragraphs = text.split(/\r?\n/);
            const parts: string[] = [];
            for (const paragraph of paragraphs) {
                if (!paragraph.trim()) {
                    parts.push(emptyLine);
                } else {
                    parts.push(this.renderLine(paragraph));
                }
            }
            return parts.join('');
        }

        // Non-paragraph mode: collapse newlines to spaces
        const singleLine = text.replace(/\r/g, '').replace(/\n/g, ' ');
        return this.renderLine(singleLine);
    }

    /**
     * Renders a single line of text using the current FIGFont and settings.
     * @param text The text to render.
     * @returns The rendered line as a string.
     */
    private renderLine(text: string): string {
        const mode = this.layoutMode === LayoutMode.Default ? LayoutMode.Smushing : this.layoutMode;

        // colorDict maps code-point position in plain text → accumulated ANSI color sequence
        const colorDict = new Map<number, string>();

        // First pass: always strip ANSI sequences; record color map only when useANSIColors is true
        {
            const processor = new ANSIProcessor(this.useANSIColors);
            let plainText = '';
            let plainIndex = 0;

            for (const c of text) {
                const isAnsi = processor.processCharacter(c);
                if (!isAnsi) {
                    if (this.useANSIColors && processor.currentColorSequence) {
                        // Map the color to the position of the next rendered character
                        colorDict.set(plainIndex, processor.currentColorSequence);
                        processor.resetColorState();
                    }
                    // Only include characters the font can render
                    if (this.font.characters.has(c)) {
                        plainText += c;
                        plainIndex++;
                    }
                }
            }

            text = plainText;
        }

        // Reverse text for RTL fonts.
        // JavaScript's spread operator correctly handles Unicode surrogate pairs.
        if (this.font.printDirection === 1) {
            const codePoints = [...text];
            text = codePoints.reverse().join('');

            // Mirror color positions around the reversed text length
            if (this.useANSIColors && colorDict.size > 0) {
                const len = codePoints.length;
                const reversed = new Map<number, string>();
                for (const [pos, color] of colorDict) {
                    reversed.set(len - pos - 1, color);
                }
                colorDict.clear();
                for (const [pos, color] of reversed) {
                    colorDict.set(pos, color);
                }
            }
        }

        const outputLines: string[] = Array(this.font.height).fill('');
        let charIndex = 0;

        // JavaScript's for...of correctly iterates Unicode code points (handles surrogates)
        for (const c of text) {
            if (!this.font.characters.has(c)) {
                charIndex++;
                continue;
            }

            const charLines = this.font.characters.get(c)!;
            const colorCode = colorDict.get(charIndex) ?? '';
            charIndex++;

            if (outputLines[0].length === 0) {
                // First character — just copy its lines, prepending any color code
                for (let i = 0; i < this.font.height; i++) {
                    outputLines[i] = colorCode + charLines[i];
                }
                continue;
            }

            // Calculate minimum overlap across all lines
            let overlap = Number.MAX_SAFE_INTEGER;
            for (let i = 0; i < this.font.height; i++) {
                overlap = Math.min(overlap, this.calculateOverlap(outputLines[i], charLines[i], mode));
            }

            // Apply smushing/kerning/full-size rules
            for (let i = 0; i < this.font.height; i++) {
                if (overlap === 0) {
                    outputLines[i] += colorCode + charLines[i];
                } else {
                    outputLines[i] = this.smushLines(outputLines[i], charLines[i], overlap, mode, colorCode);
                }
            }
        }

        const hardBlankRegex = new RegExp(this.escapeRegex(this.font.hardBlank), 'g');
        const resetCode = this.useANSIColors ? FIGLetRenderer.ANSI_RESET : '';
        const result = outputLines
            .map(line => line.replace(hardBlankRegex, ' ') + resetCode)
            .join(this.lineSeparator) + this.lineSeparator;
        return result;
    }

    /**
     * Escapes special regex characters in a string.
     */
    private escapeRegex(s: string): string {
        return s.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    }

    /**
     * Calculates the number of characters that can be overlapped between two lines.
     */
    private calculateOverlap(line: string, character: string, mode: LayoutMode): number {
        if (mode === LayoutMode.FullSize) {
            return 0;
        }

        const eol = line.length < character.length
            ? line
            : line.slice(line.length - character.length);

        const m1 = eol.match(/\S(?=\s*$)/);
        const m2Index = character.search(/\S/);
        const m2Char = m2Index >= 0 ? character[m2Index] : undefined;

        if (!m1 || m2Index === -1) {
            return character.length;
        }

        const canSmush = this.canSmush(m1[0], m2Char!, this.font.hardBlank, mode, this.font.smushingRules);
        let overlapLength = canSmush
            ? Math.max(eol.length - m1.index!, m2Index) + 1
            : 0;

        overlapLength = Math.min(overlapLength, character.length);

        // Special case when we have opposing slashes
        if ((canSmush && m1[0] === '/' && m2Char === '\\') ||
            (canSmush && m1[0] === '\\' && m2Char === '/')) {
            overlapLength = Math.max(overlapLength - 1, 0);
        }

        return overlapLength;
    }

    /**
     * Determines if two characters can be smushed together.
     * Font-agnostic: takes hardBlank and rules explicitly, matching the C# CanSmush signature.
     * @param c1 The left character.
     * @param c2 The right character.
     * @param hardBlank The hard blank character defined in the FIGFont.
     * @param mode The layout mode to use for smushing.
     * @param rules The smushing rules defined in the FIGFont.
     */
    private canSmush(c1: string, c2: string, hardBlank: string, mode: LayoutMode, rules: SmushingRules): boolean {
        // Early return for kerning mode
        if (mode === LayoutMode.Kerning) {
            return c1 === c2 && c1 === ' ';
        }

        // Early return for full size
        if (mode === LayoutMode.FullSize) {
            return false;
        }

        // Handle hardblanks first
        if (c1 === hardBlank || c2 === hardBlank) {
            return (rules & SmushingRules.HardBlank) === SmushingRules.HardBlank;
        }

        // Handle spaces
        if (c1 === ' ' && c2 === ' ') return true;
        if (c1 === ' ' || c2 === ' ') return true;

        // Rule 1: Equal Character Smushing
        if ((rules & SmushingRules.EqualCharacter) === SmushingRules.EqualCharacter && c1 === c2) {
            return true;
        }

        // Rule 2: Underscore Smushing
        if ((rules & SmushingRules.Underscore) === SmushingRules.Underscore) {
            if ((c1 === '_' && FIGLetRenderer.HIERARCHY_CHARACTERS.includes(c2)) ||
                (c2 === '_' && FIGLetRenderer.HIERARCHY_CHARACTERS.includes(c1))) {
                return true;
            }
        }

        // Rule 3: Hierarchy Smushing
        if ((rules & SmushingRules.Hierarchy) === SmushingRules.Hierarchy) {
            const rank1 = FIGLetRenderer.HIERARCHY_CHARACTERS.indexOf(c1);
            const rank2 = FIGLetRenderer.HIERARCHY_CHARACTERS.indexOf(c2);

            if (rank1 >= 0 && rank2 >= 0) {
                return true;
            }
        }

        // Rule 4: Opposite Pair Smushing
        if ((rules & SmushingRules.OppositePair) === SmushingRules.OppositePair) {
            if (FIGLetRenderer.oppositePairs.get(c1) === c2) {
                return true;
            }
        }

        // Rule 5: Big X Smushing
        if ((rules & SmushingRules.BigX) === SmushingRules.BigX) {
            if ((c1 === '/' && c2 === '\\') ||
                (c1 === '\\' && c2 === '/') ||
                (c1 === '>' && c2 === '<')) {
                return true;
            }
        }

        return false;
    }

    /**
     * Smushes two characters together.
     * Font-agnostic: takes hardBlank and rules explicitly rather than reading from this.font,
     * matching the C# SmushCharacters(char c1, char c2, char hardBlank, LayoutMode mode, SmushingRules rules) signature.
     * @param c1 The left character.
     * @param c2 The right character.
     * @param hardBlank The hard blank character defined in the FIGFont.
     * @param mode The layout mode to use for smushing.
     * @param rules The smushing rules defined in the FIGFont.
     */
    private smushCharacters(c1: string, c2: string, hardBlank: string, mode: LayoutMode, rules: SmushingRules): string {
        // Kerning: just pick the first character
        if (mode === LayoutMode.Kerning) {
            return c1;
        }

        // Handle spaces
        if (c1 === ' ' && c2 === ' ') return ' ';
        if (c1 === ' ') return c2;
        if (c2 === ' ') return c1;

        // Handle hardblanks
        if (c1 === hardBlank || c2 === hardBlank) {
            if ((rules & SmushingRules.HardBlank) === SmushingRules.HardBlank) {
                return hardBlank;
            }
            return c1;
        }

        // Rule 1: Equal Character Smushing
        if ((rules & SmushingRules.EqualCharacter) === SmushingRules.EqualCharacter && c1 === c2) {
            return c1;
        }

        // Rule 2: Underscore Smushing
        if ((rules & SmushingRules.Underscore) === SmushingRules.Underscore) {
            if (c1 === '_' && FIGLetRenderer.HIERARCHY_CHARACTERS.includes(c2)) return c2;
            if (c2 === '_' && FIGLetRenderer.HIERARCHY_CHARACTERS.includes(c1)) return c1;
        }

        // Rule 3: Hierarchy Smushing
        if ((rules & SmushingRules.Hierarchy) === SmushingRules.Hierarchy) {
            const hierarchy = FIGLetRenderer.HIERARCHY_CHARACTERS;
            const rank1 = hierarchy.indexOf(c1);
            const rank2 = hierarchy.indexOf(c2);

            if (rank1 >= 0 && rank2 >= 0) {
                return hierarchy[Math.max(rank1, rank2)];
            }
        }

        // Rule 4: Opposite Pair Smushing
        if ((rules & SmushingRules.OppositePair) === SmushingRules.OppositePair) {
            if (FIGLetRenderer.oppositePairs.get(c1) === c2) {
                return '|';
            }
        }

        // Rule 5: Big X Smushing
        if ((rules & SmushingRules.BigX) === SmushingRules.BigX) {
            if (c1 === '/' && c2 === '\\') return '|';
            if (c1 === '\\' && c2 === '/') return 'Y';
            if (c1 === '>' && c2 === '<') return 'X';
        }

        // Rule 6: Hardblank Smushing
        if ((rules & SmushingRules.HardBlank) === SmushingRules.HardBlank) {
            if (c1 === hardBlank && c2 === hardBlank) {
                return hardBlank;
            }
        }

        // No rule matched — keep the left character
        return c1;
    }

    /**
     * Smushes a character line into another line at a specific overlap point.
     * The colorCode, if provided, is inserted after the smushed overlap and before
     * the remaining tail of the incoming character — matching C# behaviour.
     */
    private smushLines(line: string, character: string, overlap: number, mode: LayoutMode, colorCode: string = ''): string {
        const lineEnd = line.slice(-overlap);
        const lineWithoutEnd = line.slice(0, -overlap);

        if (mode === LayoutMode.Kerning) {
            return line + colorCode + character;
        }

        let smushedPart = '';
        for (let i = 0; i < overlap; i++) {
            smushedPart += this.smushCharacters(lineEnd[i], character[i], this.font.hardBlank, mode, this.font.smushingRules);
        }

        return lineWithoutEnd + smushedPart + colorCode + character.slice(overlap);
    }
}

// ---------------------------------------------------------------------------
// ANSIProcessor — detects and buffers ANSI escape sequences during rendering.
// Ported from the C# inner class of the same name in FIGLetRenderer.cs.
// ---------------------------------------------------------------------------

class ANSIProcessor {
    /** Whether to accumulate color sequences (only needed when useANSIColors is true). */
    private readonly preserveColors: boolean;

    private inEscapeSequence = false;
    private escapeBuffer = '';

    constructor(preserveColors = false) {
        this.preserveColors = preserveColors;
    }

    /**
     * Terminal control characters that end an ANSI sequence but are NOT color
     * codes (i.e. the sequence should be dropped rather than preserved).
     */
    private static readonly NON_COLOR_TERMINATORS = new Set([
        'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'f',   // cursor movement
        'J', 'K',                                         // screen clearing
        'S', 'T',                                         // scrolling
        's', 'u',                                         // cursor save/restore
        'n', 'h', 'l', 'i', 'r', 't', '@',               // misc controls
        'P', 'X', 'L', 'M'                                // more controls
    ]);

    /**
     * The accumulated color sequence for the current run of color codes.
     * Resets after each non-ANSI character is processed.
     */
    public currentColorSequence = '';

    /**
     * Processes a single character.
     * @returns true if the character was part of an escape sequence (caller should skip it);
     *          false if it is a regular printable character.
     */
    public processCharacter(c: string): boolean {
        // Start of a new escape sequence
        if (c === '\u001b') {
            this.inEscapeSequence = true;
            this.escapeBuffer = c;
            return true;
        }

        if (this.inEscapeSequence) {
            this.escapeBuffer += c;

            // Must be a CSI sequence (ESC + '['); anything else is discarded
            if (this.escapeBuffer.length === 2 && c !== '[') {
                this.inEscapeSequence = false;
                this.escapeBuffer = '';
                return true;
            }

            // Sequence is complete when a final byte in range 0x40–0x7E is seen
            if (this.escapeBuffer.length >= 3 &&
                ((c >= '\x40' && c <= '\x7e') ||
                 ANSIProcessor.NON_COLOR_TERMINATORS.has(c) ||
                 c === 'm')) {

                this.inEscapeSequence = false;
                const sequence = this.escapeBuffer;
                this.escapeBuffer = '';

                // Only preserve color sequences (those ending with 'm') when enabled
                if (c === 'm' && this.preserveColors) {
                    this.currentColorSequence += sequence;
                }

                return true;
            }

            // Still inside the sequence
            return true;
        }

        return false;
    }

    /**
     * Clears the accumulated color state after it has been recorded in the color map.
     */
    public resetColorState(): void {
        this.currentColorSequence = '';
    }
}
