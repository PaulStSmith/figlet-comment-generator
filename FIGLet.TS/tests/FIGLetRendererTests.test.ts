import { describe, it, expect, beforeEach } from 'vitest';
import { FIGFont } from '../src/FIGFont.js';
import { FIGLetRenderer } from '../src/FIGLetRenderer.js';
import { LayoutMode } from '../src/LayoutMode.js';
import { SmushingRules } from '../src/SmushingRules.js';
import {
    assertMultiLineEqual,
    createMinimalValidFontContent,
    loadTestFont,
    generateLargeText,
    stripANSIColors,
    testTexts,
    ansiColoredTexts,
} from './TestUtilities.js';

// ---------------------------------------------------------------------------
// FIGLetRenderer tests — mirrors C# FIGLetRendererTests
// ---------------------------------------------------------------------------

describe('FIGLetRenderer', () => {
    let testFont: FIGFont;
    let smushingFont: FIGFont;

    beforeEach(() => {
        testFont = loadTestFont('mini-fixed');
        smushingFont = loadTestFont('smushing-test');
    });

    // -----------------------------------------------------------------------
    // Constructor
    // -----------------------------------------------------------------------

    describe('constructor', () => {
        it('should use default parameters', () => {
            const renderer = new FIGLetRenderer(testFont);
            expect(renderer.font).toBe(testFont);
            expect(renderer.layoutMode).toBe(LayoutMode.Default);
            expect(renderer.lineSeparator).toBe('\n');
            expect(renderer.useANSIColors).toBe(false);
            expect(renderer.paragraphMode).toBe(true);
        });

        it('should accept custom parameters', () => {
            const renderer = new FIGLetRenderer(testFont, LayoutMode.Kerning, '|', true, false);
            expect(renderer.font).toBe(testFont);
            expect(renderer.layoutMode).toBe(LayoutMode.Kerning);
            expect(renderer.lineSeparator).toBe('|');
            expect(renderer.useANSIColors).toBe(true);
            expect(renderer.paragraphMode).toBe(false);
        });
    });

    // -----------------------------------------------------------------------
    // Static render
    // -----------------------------------------------------------------------

    describe('static render', () => {
        it('should return empty string for null/empty text', () => {
            expect(FIGLetRenderer.render('', testFont)).toBe('');
        });

        it('should render text correctly', () => {
            const result = FIGLetRenderer.render('Hi', testFont, LayoutMode.Default, '\n');
            expect(result).not.toBeNull();
            assertMultiLineEqual('Hi\nHi\nHi\n', result);
            const lines = result.split('\n').filter(l => l.length > 0);
            expect(lines.length).toBe(testFont.height);
        });
    });

    // -----------------------------------------------------------------------
    // Instance render — basic
    // -----------------------------------------------------------------------

    describe('render', () => {
        it('should return empty string for empty input', () => {
            const renderer = new FIGLetRenderer(testFont);
            expect(renderer.render('')).toBe('');
        });

        it('should render a single character', () => {
            const renderer = new FIGLetRenderer(testFont);
            const result = renderer.render('A');
            expect(result).not.toBeNull();
            const lines = result.split('\n').filter(l => l.length > 0);
            expect(lines.length).toBe(testFont.height);
            expect(result.replace(new RegExp(`\\${testFont.hardBlank}`, 'g'), ' ')).toContain('A');
        });

        it('should be wider for two characters than for one', () => {
            const renderer = new FIGLetRenderer(testFont);
            const two = renderer.render('AB').split('\n').filter(l => l.length > 0);
            const one = renderer.render('A').split('\n').filter(l => l.length > 0);
            expect(two[0].length).toBeGreaterThan(one[0].length);
        });

        // -----------------------------------------------------------------------
        // Layout modes
        // -----------------------------------------------------------------------

        it.each([LayoutMode.FullSize, LayoutMode.Kerning, LayoutMode.Smushing])(
            'should produce output of correct height for mode %s',
            (mode) => {
                const renderer = new FIGLetRenderer(testFont, mode);
                const result = renderer.render('AB');
                const lines = result.split('\n').filter(l => l.length > 0);
                expect(lines.length).toBe(testFont.height);
            }
        );

        it('FullSize should have no overlap (width = sum of individual widths)', () => {
            const renderer = new FIGLetRenderer(testFont, LayoutMode.FullSize);
            const twoLines = renderer.render('AB').split('\n').filter(l => l.length > 0);
            const aLines   = renderer.render('A').split('\n').filter(l => l.length > 0);
            const bLines   = renderer.render('B').split('\n').filter(l => l.length > 0);
            expect(twoLines[0].length).toBe(aLines[0].length + bLines[0].length);
        });

        it('widths should decrease: FullSize >= Kerning >= Smushing', () => {
            const full  = new FIGLetRenderer(testFont, LayoutMode.FullSize).render('AB')
                .split('\n').filter(l => l.length > 0);
            const kern  = new FIGLetRenderer(testFont, LayoutMode.Kerning).render('AB')
                .split('\n').filter(l => l.length > 0);
            const smush = new FIGLetRenderer(testFont, LayoutMode.Smushing).render('AB')
                .split('\n').filter(l => l.length > 0);
            expect(full[0].length).toBeGreaterThanOrEqual(kern[0].length);
            expect(kern[0].length).toBeGreaterThanOrEqual(smush[0].length);
        });

        // -----------------------------------------------------------------------
        // Paragraph mode
        // -----------------------------------------------------------------------

        it('paragraph mode should expand newlines to separate renders', () => {
            const renderer = new FIGLetRenderer(testFont, LayoutMode.Default, '\n', false, true);
            const result = renderer.render('A\nB');
            const lines = result.split('\n').filter(l => l.length > 0);
            expect(lines.length).toBeGreaterThanOrEqual(testFont.height * 2);
        });

        it('non-paragraph mode should collapse newlines to spaces', () => {
            const renderer = new FIGLetRenderer(testFont, LayoutMode.Default, '\n', false, false);
            const result = renderer.render('A\nB');
            const lines = result.split('\n').filter(l => l.length > 0);
            expect(lines.length).toBe(testFont.height);
        });

        it('paragraph mode: blank-line separator adds exactly font.height extra newlines', () => {
            const renderer = new FIGLetRenderer(testFont, LayoutMode.Default, '\n', false, true);
            const consecutive = renderer.render('A\nB');   // no blank line
            const separated   = renderer.render('A\n\nB'); // blank line between

            const stripped = consecutive.replace(new RegExp(`\\${testFont.hardBlank}`, 'g'), ' ');
            expect(stripped).toContain('A');
            expect(stripped).toContain('B');

            const consecutiveNL = (consecutive.match(/\n/g) ?? []).length;
            const separatedNL   = (separated.match(/\n/g) ?? []).length;
            expect(separatedNL - consecutiveNL).toBe(testFont.height);
        });

        // -----------------------------------------------------------------------
        // Custom line separator
        // -----------------------------------------------------------------------

        it('should use custom line separator', () => {
            const renderer = new FIGLetRenderer(testFont, LayoutMode.Default, '|');
            const result = renderer.render('A');
            expect(result).toContain('|');
            expect(result).not.toContain('\n');
        });

        // -----------------------------------------------------------------------
        // ANSI colour handling
        // -----------------------------------------------------------------------

        it('should preserve ANSI colors when useANSIColors = true', () => {
            const renderer = new FIGLetRenderer(testFont, LayoutMode.Default, '\n', true);
            const result = renderer.render('\x1b[31mR\x1b[32mG\x1b[0m');
            expect(result).toContain('\x1b[31m');
            expect(result).toContain('\x1b[32m');
            expect(result).toContain('\x1b[0m');
        });

        it('should strip ANSI and not include escape sequences when useANSIColors = false', () => {
            const renderer = new FIGLetRenderer(testFont, LayoutMode.Default, '\n', false);
            const result = renderer.render('\x1b[31mRed\x1b[0m');
            expect(result).not.toContain('\x1b[');
            expect(result).toContain('Red');
        });

        it('should handle complex ANSI sequences correctly', () => {
            const renderer = new FIGLetRenderer(testFont, LayoutMode.Default, '\n', true);
            const input = '\x1b[38;5;196m\x1b[48;2;255;255;0m\x1b[1m\x1b[4mTest\x1b[0m';
            const result = renderer.render(input);
            expect(result).toContain('\x1b[38;5;196m');
            expect(result).toContain('\x1b[48;2;255;255;0m');
            expect(result).toContain('\x1b[1m');
            expect(result).toContain('\x1b[4m');
            expect(result).toContain('\x1b[0m');
        });

        // -----------------------------------------------------------------------
        // RTL
        // -----------------------------------------------------------------------

        it('RTL font should have printDirection = 1', () => {
            const content = createMinimalValidFontContent(2, '$', 1);
            const rtlFont = FIGFont.fromText(content)!;
            expect(rtlFont.printDirection).toBe(1);
        });

        it('RTL render of "AB" (stripped) should equal LTR render of "BA"', () => {
            const ltrContent = createMinimalValidFontContent(2, '$', 0);
            const rtlContent = createMinimalValidFontContent(2, '$', 1);
            const ltrFont = FIGFont.fromText(ltrContent)!;
            const rtlFont = FIGFont.fromText(rtlContent)!;

            const ltrRenderer = new FIGLetRenderer(ltrFont, LayoutMode.Default, '\n');
            const rtlRenderer = new FIGLetRenderer(rtlFont, LayoutMode.Default, '\n', true);

            const rtlResult = rtlRenderer.render('\x1b[31mAB\x1b[0m');

            // ANSI sequences are preserved
            expect(rtlResult).toContain('\x1b[31m');
            expect(rtlResult).toContain('\x1b[0m');

            // Text content (stripped) equals LTR render of reversed text
            const stripped = stripANSIColors(rtlResult);
            const ltrBA = ltrRenderer.render('BA');
            expect(stripped).toBe(ltrBA);
        });

        // -----------------------------------------------------------------------
        // Unicode / missing characters
        // -----------------------------------------------------------------------

        it('should skip unknown characters gracefully', () => {
            const renderer = new FIGLetRenderer(testFont);
            const result = renderer.render('A\u2603B'); // snowman not in font
            const processed = result.replace(new RegExp(`\\${testFont.hardBlank}`, 'g'), ' ');
            expect(processed).toContain('A');
            expect(processed).toContain('B');
        });

        it('should handle surrogate-pair emoji gracefully', () => {
            const renderer = new FIGLetRenderer(testFont);
            const result = renderer.render('A\uD83D\uDE80B'); // rocket emoji
            const processed = result.replace(new RegExp(`\\${testFont.hardBlank}`, 'g'), ' ');
            expect(processed).toContain('A');
            expect(processed).toContain('B');
        });

        // -----------------------------------------------------------------------
        // Batch test vectors
        // -----------------------------------------------------------------------

        it('should handle all testTexts cases correctly', () => {
            const renderer = new FIGLetRenderer(testFont, LayoutMode.Default, '\n');
            for (const [input, expected] of Object.entries(testTexts)) {
                const result = renderer.render(input);
                expect(result, `Failed to find expected value for: "${input}"`).toContain(expected);
            }
        });

        it('should handle all ANSI colored text cases correctly', () => {
            const renderer = new FIGLetRenderer(testFont, LayoutMode.Default, '\n', true);
            for (const [input, expected] of Object.entries(ansiColoredTexts)) {
                const result = renderer.render(input);
                expect(result, `Failed for: "${input.replace(/\x1b/g, '\\x1b')}"`).toContain(expected);
            }
        });

        // -----------------------------------------------------------------------
        // Performance
        // -----------------------------------------------------------------------

        it('should render 100 random characters within 5 seconds', () => {
            const renderer = new FIGLetRenderer(testFont);
            const text = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789'.repeat(3).slice(0, 100);
            const start = Date.now();
            const result = renderer.render(text);
            const elapsed = Date.now() - start;
            expect(result).toBeTruthy();
            expect(elapsed).toBeLessThan(5000);
        });
    });

    // -----------------------------------------------------------------------
    // smushCharacters (private — accessed via type cast)
    // mirrors C# SmushingRules_* tests
    // -----------------------------------------------------------------------

    describe('smushCharacters', () => {
        // Helper: call the private method through a type cast
        const smush = (
            r: FIGLetRenderer,
            c1: string,
            c2: string,
            hardBlank: string,
            mode: LayoutMode,
            rules: SmushingRules
        ): string => (r as unknown as Record<string, Function>)['smushCharacters'](c1, c2, hardBlank, mode, rules);

        it('EqualCharacter — identical chars smush to that char', () => {
            const renderer = new FIGLetRenderer(smushingFont);
            expect(smush(renderer, '|', '|', '$', LayoutMode.Smushing, SmushingRules.EqualCharacter)).toBe('|');
        });

        it('Underscore — underscore yields to hierarchy char', () => {
            const renderer = new FIGLetRenderer(smushingFont);
            expect(smush(renderer, '_', '|', '$', LayoutMode.Smushing, SmushingRules.Underscore)).toBe('|');
        });

        it('Hierarchy — higher-ranked char wins', () => {
            const renderer = new FIGLetRenderer(smushingFont);
            expect(smush(renderer, '|', '/', '$', LayoutMode.Smushing, SmushingRules.Hierarchy)).toBe('/');
        });

        it('OppositePair — [ and ] produce |', () => {
            const renderer = new FIGLetRenderer(smushingFont);
            expect(smush(renderer, '[', ']', '$', LayoutMode.Smushing, SmushingRules.OppositePair)).toBe('|');
        });

        it('BigX — / and \\ produce |', () => {
            const renderer = new FIGLetRenderer(smushingFont);
            expect(smush(renderer, '/', '\\', '$', LayoutMode.Smushing, SmushingRules.BigX)).toBe('|');
        });

        it('BigX — \\ and / produce Y', () => {
            const renderer = new FIGLetRenderer(smushingFont);
            expect(smush(renderer, '\\', '/', '$', LayoutMode.Smushing, SmushingRules.BigX)).toBe('Y');
        });

        it('BigX — > and < produce X', () => {
            const renderer = new FIGLetRenderer(smushingFont);
            expect(smush(renderer, '>', '<', '$', LayoutMode.Smushing, SmushingRules.BigX)).toBe('X');
        });

        it('HardBlank — two hard-blanks smush to hard-blank', () => {
            const renderer = new FIGLetRenderer(smushingFont);
            const hb = smushingFont.hardBlank;
            expect(smush(renderer, hb, hb, hb, LayoutMode.Smushing, SmushingRules.HardBlank)).toBe(hb);
        });
    });

    // -----------------------------------------------------------------------
    // canSmush (private — accessed via type cast)
    // -----------------------------------------------------------------------

    describe('canSmush', () => {
        const canSmush = (
            r: FIGLetRenderer,
            c1: string,
            c2: string,
            hardBlank: string,
            mode: LayoutMode,
            rules: SmushingRules
        ): boolean => (r as unknown as Record<string, Function>)['canSmush'](c1, c2, hardBlank, mode, rules);

        it('spaces should always be smushable', () => {
            const renderer = new FIGLetRenderer(testFont);
            expect(canSmush(renderer, ' ', ' ', '$', LayoutMode.Smushing, SmushingRules.None)).toBe(true);
        });

        it('Kerning mode: only space+space returns true, all other pairs false', () => {
            const renderer = new FIGLetRenderer(testFont);
            // c1 === c2 && c1 === ' '  →  true
            expect(canSmush(renderer, ' ', ' ', '$', LayoutMode.Kerning, SmushingRules.None)).toBe(true);
            // identical non-space, or different chars  →  false
            expect(canSmush(renderer, 'A', 'A', '$', LayoutMode.Kerning, SmushingRules.None)).toBe(false);
            expect(canSmush(renderer, 'A', 'B', '$', LayoutMode.Kerning, SmushingRules.None)).toBe(false);
        });

        it('FullSize mode cannot smush', () => {
            const renderer = new FIGLetRenderer(testFont);
            expect(canSmush(renderer, 'A', 'A', '$', LayoutMode.FullSize, SmushingRules.EqualCharacter)).toBe(false);
        });
    });
});
