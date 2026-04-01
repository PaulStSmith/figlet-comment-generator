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
     * Gets or sets a value indicating whether to use ANSI colors during rendering.
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

        // Reverse text for RTL fonts.
        // JavaScript's spread operator correctly handles Unicode surrogate pairs,
        // so no explicit surrogate pair handling is needed here.
        if (this.font.printDirection === 1) {
            text = [...text].reverse().join('');
        }

        const outputLines: string[] = Array(this.font.height).fill('');

        // JavaScript's for...of correctly iterates Unicode code points (handles surrogates)
        for (const c of text) {
            if (!this.font.characters.has(c)) {
                continue;
            }

            const charLines = this.font.characters.get(c)!;
            if (outputLines[0].length === 0) {
                // First character — just copy its lines
                for (let i = 0; i < this.font.height; i++) {
                    outputLines[i] = charLines[i];
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
                    outputLines[i] += charLines[i];
                } else {
                    outputLines[i] = this.smushLines(outputLines[i], charLines[i], overlap, mode);
                }
            }
        }

        const hardBlankRegex = new RegExp(this.escapeRegex(this.font.hardBlank), 'g');
        return outputLines
            .map(line => line.replace(hardBlankRegex, ' '))
            .join(this.lineSeparator);
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
        const m2 = character.match(/(?<=^\s*)\S/);

        if (!m1 || !m2) {
            return character.length;
        }

        const canSmush = this.canSmush(m1[0], m2[0], mode);
        let overlapLength = canSmush
            ? Math.max(eol.length - m1.index!, m2.index!) + 1
            : 0;

        overlapLength = Math.min(overlapLength, character.length);

        // Special case when we have opposing slashes
        if ((canSmush && m1[0] === '/' && m2[0] === '\\') ||
            (canSmush && m1[0] === '\\' && m2[0] === '/')) {
            overlapLength = Math.max(overlapLength - 1, 0);
        }

        return overlapLength;
    }

    /**
     * Determines if two characters can be smushed together.
     */
    private canSmush(c1: string, c2: string, mode: LayoutMode): boolean {
        // Early return for kerning mode
        if (mode === LayoutMode.Kerning) {
            return c1 === c2 && c1 === ' ';
        }

        // Early return for full size
        if (mode === LayoutMode.FullSize) {
            return false;
        }

        // Handle hardblanks first
        if (c1 === this.font.hardBlank || c2 === this.font.hardBlank) {
            return this.font.hasSmushingRule(SmushingRules.HardBlank);
        }

        // Handle spaces
        if (c1 === ' ' && c2 === ' ') return true;
        if (c1 === ' ' || c2 === ' ') return true;

        // Rule 1: Equal Character Smushing
        if (this.font.hasSmushingRule(SmushingRules.EqualCharacter) && c1 === c2) {
            return true;
        }

        // Rule 2: Underscore Smushing
        if (this.font.hasSmushingRule(SmushingRules.Underscore)) {
            if ((c1 === '_' && FIGLetRenderer.HIERARCHY_CHARACTERS.includes(c2)) ||
                (c2 === '_' && FIGLetRenderer.HIERARCHY_CHARACTERS.includes(c1))) {
                return true;
            }
        }

        // Rule 3: Hierarchy Smushing
        if (this.font.hasSmushingRule(SmushingRules.Hierarchy)) {
            const hierarchy = FIGLetRenderer.HIERARCHY_CHARACTERS;
            const rank1 = hierarchy.indexOf(c1);
            const rank2 = hierarchy.indexOf(c2);

            if (rank1 >= 0 && rank2 >= 0) {
                return true;
            }
        }

        // Rule 4: Opposite Pair Smushing
        if (this.font.hasSmushingRule(SmushingRules.OppositePair)) {
            if (FIGLetRenderer.oppositePairs.get(c1) === c2) {
                return true;
            }
        }

        // Rule 5: Big X Smushing
        if (this.font.hasSmushingRule(SmushingRules.BigX)) {
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
     */
    private smushCharacters(c1: string, c2: string, mode: LayoutMode): string {
        // Kerning: just pick the first character
        if (mode === LayoutMode.Kerning) {
            return c1;
        }

        // Handle spaces
        if (c1 === ' ' && c2 === ' ') return ' ';
        if (c1 === ' ') return c2;
        if (c2 === ' ') return c1;

        // Handle hardblanks
        if (c1 === this.font.hardBlank || c2 === this.font.hardBlank) {
            if (this.font.hasSmushingRule(SmushingRules.HardBlank)) {
                return this.font.hardBlank;
            }
            return c1;
        }

        // Rule 1: Equal Character Smushing
        if (this.font.hasSmushingRule(SmushingRules.EqualCharacter) && c1 === c2) {
            return c1;
        }

        // Rule 2: Underscore Smushing
        if (this.font.hasSmushingRule(SmushingRules.Underscore)) {
            if (c1 === '_' && FIGLetRenderer.HIERARCHY_CHARACTERS.includes(c2)) return c2;
            if (c2 === '_' && FIGLetRenderer.HIERARCHY_CHARACTERS.includes(c1)) return c1;
        }

        // Rule 3: Hierarchy Smushing
        if (this.font.hasSmushingRule(SmushingRules.Hierarchy)) {
            const hierarchy = FIGLetRenderer.HIERARCHY_CHARACTERS;
            const rank1 = hierarchy.indexOf(c1);
            const rank2 = hierarchy.indexOf(c2);

            if (rank1 >= 0 && rank2 >= 0) {
                return hierarchy[Math.max(rank1, rank2)];
            }
        }

        // Rule 4: Opposite Pair Smushing
        if (this.font.hasSmushingRule(SmushingRules.OppositePair)) {
            if (FIGLetRenderer.oppositePairs.get(c1) === c2) {
                return '|';
            }
        }

        // Rule 5: Big X Smushing
        if (this.font.hasSmushingRule(SmushingRules.BigX)) {
            if ((c1 === '/' && c2 === '\\') || (c1 === '\\' && c2 === '/')) {
                return '|';
            }
            if (c1 === '>' && c2 === '<') {
                return 'X';
            }
        }

        // Rule 6: Hardblank Smushing
        if (this.font.hasSmushingRule(SmushingRules.HardBlank)) {
            if (c1 === this.font.hardBlank && c2 === this.font.hardBlank) {
                return this.font.hardBlank;
            }
        }

        // No rule matched — keep the left character
        return c1;
    }

    /**
     * Smushes a character line into another line at a specific overlap point.
     */
    private smushLines(line: string, character: string, overlap: number, mode: LayoutMode): string {
        const lineEnd = line.slice(-overlap);
        const lineWithoutEnd = line.slice(0, -overlap);

        if (mode === LayoutMode.Kerning) {
            return line + character;
        }

        let smushedPart = '';
        for (let i = 0; i < overlap; i++) {
            smushedPart += this.smushCharacters(lineEnd[i], character[i], mode);
        }

        return lineWithoutEnd + smushedPart + character.slice(overlap);
    }
}
