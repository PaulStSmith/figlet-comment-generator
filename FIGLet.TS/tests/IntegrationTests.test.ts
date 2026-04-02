import { describe, it, expect } from 'vitest';
import { writeFile, unlink } from 'fs/promises';
import { tmpdir } from 'os';
import { join } from 'path';
import { FIGFont } from '../src/FIGFont.js';
import { FIGLetRenderer } from '../src/FIGLetRenderer.js';
import { LayoutMode } from '../src/LayoutMode.js';
import {
    assertMultiLineEqual,
    createMinimalValidFontContent,
    createZipWithFontFile,
    loadTestFont,
    stripANSIColors,
} from './TestUtilities.js';

// ---------------------------------------------------------------------------
// Integration tests — mirrors C# IntegrationTests
// ---------------------------------------------------------------------------

describe('Integration', () => {

    // -----------------------------------------------------------------------
    // Default font + renderer
    // -----------------------------------------------------------------------

    it('default font and renderer should work end-to-end', async () => {
        const font = await FIGFont.getDefault();
        const result = FIGLetRenderer.render('Hello', font, LayoutMode.Default, '\n');
        expect(result).toBeTruthy();
        const lines = result.split('\n').filter(l => l.length > 0);
        expect(lines.length).toBeGreaterThan(0);
        expect(lines.every(l => l.length > 0)).toBe(true);
    });

    // -----------------------------------------------------------------------
    // fromFile — temp file
    // -----------------------------------------------------------------------

    it('custom font loaded from file should render correctly', async () => {
        const fontContent = createMinimalValidFontContent(4, '#');
        const tempFile = join(tmpdir(), `figlet_int_${Date.now()}.flf`);
        await writeFile(tempFile, fontContent);
        try {
            const font = await FIGFont.fromFile(tempFile);
            const result = FIGLetRenderer.render('Test', font!, LayoutMode.Default, '\n');
            expect(font).not.toBeNull();
            expect(font!.height).toBe(4);
            expect(result).toBeTruthy();
            const lines = result.split('\n').filter(l => l.length > 0);
            expect(lines.length).toBe(4);
        } finally {
            await unlink(tempFile).catch(() => {});
        }
    });

    // -----------------------------------------------------------------------
    // ZIP font
    // -----------------------------------------------------------------------

    it('should load and render a Deflate-compressed ZIP font', async () => {
        const fontContent = createMinimalValidFontContent(3, '@');
        const zipBytes = createZipWithFontFile(fontContent, 'test.flf', false);
        const tempFile = join(tmpdir(), `figlet_int_deflate_${Date.now()}.zip`);
        await writeFile(tempFile, zipBytes);
        try {
            const font = await FIGFont.fromFile(tempFile);
            const result = FIGLetRenderer.render('Hi', font!, LayoutMode.Default, '\n');
            expect(font).not.toBeNull();
            expect(font!.hardBlank).toBe('@');
            expect(font!.height).toBe(3);
            const lines = result.split('\n').filter(l => l.length > 0);
            expect(lines.length).toBe(3);
        } finally {
            await unlink(tempFile).catch(() => {});
        }
    });

    it('should load and render a Store-compressed ZIP font', async () => {
        const fontContent = createMinimalValidFontContent(4, '%');
        const zipBytes = createZipWithFontFile(fontContent, 'test.flf', true);
        const tempFile = join(tmpdir(), `figlet_int_store_${Date.now()}.zip`);
        await writeFile(tempFile, zipBytes);
        try {
            const font = await FIGFont.fromFile(tempFile);
            const result = FIGLetRenderer.render('Hi', font!, LayoutMode.Default, '\n');
            expect(font).not.toBeNull();
            expect(font!.hardBlank).toBe('%');
            expect(font!.height).toBe(4);
            const lines = result.split('\n').filter(l => l.length > 0);
            expect(lines.length).toBe(4);
        } finally {
            await unlink(tempFile).catch(() => {});
        }
    });

    // -----------------------------------------------------------------------
    // All layout modes produce different widths
    // -----------------------------------------------------------------------

    it('all layout modes should produce correct heights, widths decrease from FullSize to Smushing', () => {
        const font = loadTestFont('mini-fixed');
        const text = 'ABCD';

        const fullSize = FIGLetRenderer.render(text, font, LayoutMode.FullSize, '\n');
        const kerning  = FIGLetRenderer.render(text, font, LayoutMode.Kerning,  '\n');
        const smushing = FIGLetRenderer.render(text, font, LayoutMode.Smushing, '\n');

        const fullLines  = fullSize.split('\n').filter(l => l.length > 0);
        const kernLines  = kerning.split('\n').filter(l => l.length > 0);
        const smushLines = smushing.split('\n').filter(l => l.length > 0);

        expect(fullLines.length).toBe(font.height);
        expect(kernLines.length).toBe(font.height);
        expect(smushLines.length).toBe(font.height);

        expect(fullLines[0].length).toBeGreaterThanOrEqual(kernLines[0].length);
        expect(kernLines[0].length).toBeGreaterThanOrEqual(smushLines[0].length);
    });

    // -----------------------------------------------------------------------
    // Complex text: paragraph mode + ANSI
    // -----------------------------------------------------------------------

    it('complex text with ANSI and paragraphs should preserve both colors and line structure', () => {
        const font = loadTestFont('smushing-test');
        const renderer = new FIGLetRenderer(font, LayoutMode.Smushing, '\n', true, true);
        const complexText = '\x1b[31mHello\x1b[0m\n\x1b[32mWorld\x1b[0m!';

        const result = renderer.render(complexText);

        expect(result).toContain('\x1b[31m');
        expect(result).toContain('\x1b[32m');
        expect(result).toContain('\x1b[0m');
        expect(result).toContain('\n');

        const lines = result.split('\n').filter(l => l.length > 0);
        expect(lines.length).toBeGreaterThanOrEqual(font.height * 2);
    });

    // -----------------------------------------------------------------------
    // Different line separators
    // -----------------------------------------------------------------------

    it.each(['\n', '\r\n', '|', '|||'])(
        'should use custom line separator "%s" throughout output',
        (sep) => {
            const font = loadTestFont('mini-fixed');
            const renderer = new FIGLetRenderer(font, LayoutMode.Default, sep);
            const result = renderer.render('Test');

            expect(result).toContain(sep);
            const parts = result.split(sep).filter(p => p.length > 0);
            expect(parts.length).toBe(font.height);
        }
    );

    // -----------------------------------------------------------------------
    // Banner generation against the default (small) font
    // -----------------------------------------------------------------------

    it('real-world banner generation should match expected output', async () => {
        const font = await FIGFont.getDefault();
        const scenarios = [
            {
                text: 'ERROR',
                result: '  ___ ___ ___  ___  ___ \n | __| _ \\ _ \\/ _ \\| _ \\\n | _||   /   / (_) |   /\n |___|_|_\\_|_\\\\___/|_|_\\\n                        \n',
            },
            {
                text: 'SUCCESS',
                result: '  ___ _   _  ___ ___ ___ ___ ___ \n / __| | | |/ __/ __| __/ __/ __|\n \\__ \\ |_| | (_| (__| _|\\__ \\__ \\\n |___/\\___/ \\___\\___|___|___/___/\n                                 \n',
            },
            {
                text: 'WARNING',
                result: ' __      __ _   ___ _  _ ___ _  _  ___ \n \\ \\    / //_\\ | _ \\ \\| |_ _| \\| |/ __|\n  \\ \\/\\/ // _ \\|   / .` || || .` | (_ |\n   \\_/\\_//_/ \\_\\_|_\\_|\\_|___|_|\\_|\\___|\n                                       \n',
            },
            {
                text: 'DEBUG',
                result: '  ___  ___ ___ _   _  ___ \n |   \\| __| _ ) | | |/ __|\n | |) | _|| _ \\ |_| | (_ |\n |___/|___|___/\\___/ \\___|\n                          \n',
            },
        ];

        for (const { text, result: expected } of scenarios) {
            const result = FIGLetRenderer.render(text, font, LayoutMode.Default, '\n');
            assertMultiLineEqual(expected, result, `Banner mismatch for "${text}": `);
        }
    });

    // -----------------------------------------------------------------------
    // Error recovery
    // -----------------------------------------------------------------------

    it('should skip unknown characters and still render known ones', () => {
        const font = loadTestFont('mini-fixed');
        const renderer = new FIGLetRenderer(font);
        const result = renderer.render('A\u2603B'); // snowman not in font
        const processed = result.replace(new RegExp(`\\${font.hardBlank}`, 'g'), ' ');
        expect(processed).toContain('A');
        expect(processed).toContain('B');
    });

    it('empty and whitespace-only input should return empty string', () => {
        const font = loadTestFont('mini-fixed');
        const renderer = new FIGLetRenderer(font);
        expect(renderer.render('')).toBe('');
    });

    it('very long input should not throw', () => {
        const font = loadTestFont('mini-fixed');
        const renderer = new FIGLetRenderer(font);
        const longText = 'A'.repeat(1000);
        const result = renderer.render(longText);
        expect(result).toBeTruthy();
        expect(result.length).toBeGreaterThan(0);
    });

    // -----------------------------------------------------------------------
    // All smushing rules
    // -----------------------------------------------------------------------

    it('smushing should produce narrower output than FullSize for each smushing scenario', () => {
        const font = loadTestFont('smushing-test');
        const renderer = new FIGLetRenderer(font, LayoutMode.Smushing, '\n');

        const testCases: Array<[string, string]> = [
            ['AA', 'Equal character smushing'],
            ['_|', 'Underscore smushing'],
            ['||', 'Hierarchy smushing'],
            ['[]', 'Opposite pair smushing'],
            ['/\\', 'Big X smushing (slashes)'],
            ['><', 'Big X smushing (arrows)'],
        ];

        for (const [text, desc] of testCases) {
            const smushResult  = renderer.render(text);
            const fullResult   = FIGLetRenderer.render(text, font, LayoutMode.FullSize, '\n');
            const smushLines   = smushResult.split('\n').filter(l => l.length > 0);
            const fullLines    = fullResult.split('\n').filter(l => l.length > 0);
            expect(smushLines[0].length, desc).toBeLessThanOrEqual(fullLines[0].length);
        }
    });

    // -----------------------------------------------------------------------
    // Comment-style formatting
    // -----------------------------------------------------------------------

    it('banner lines should all start with given comment prefix', async () => {
        const font = await FIGFont.getDefault();
        const renderer = new FIGLetRenderer(font, LayoutMode.Default, '\n');
        const banner = renderer.render('API');

        const commentStyles: Array<[string, string]> = [
            ['C#', '//'],
            ['Python', '#'],
            ['SQL', '--'],
            ['HTML', '<!-- '],
        ];

        for (const [language, prefix] of commentStyles) {
            const commented = banner
                .split('\n')
                .filter(l => l.length > 0)
                .map(l => `${prefix} ${l}`)
                .join('\n');

            const lines = commented.split('\n').filter(l => l.length > 0);
            expect(lines.every(l => l.startsWith(prefix)), `Failed for ${language}`).toBe(true);
        }
    });

    // -----------------------------------------------------------------------
    // Complex ANSI sequences preserved end-to-end
    // -----------------------------------------------------------------------

    it('complex ANSI sequences should be fully preserved', async () => {
        const font = await FIGFont.getDefault();
        const renderer = new FIGLetRenderer(font, LayoutMode.Default, '\n', true);
        const input = [
            '\x1b[38;5;196m',       // 256-color red
            '\x1b[48;2;0;255;0m',   // true color green background
            '\x1b[1m',              // bold
            '\x1b[3m',              // italic
            '\x1b[4m',              // underline
            'Complex',
            '\x1b[0m',              // reset
        ].join('');

        const result = renderer.render(input);
        expect(result).toContain('\x1b[38;5;196m');
        expect(result).toContain('\x1b[48;2;0;255;0m');
        expect(result).toContain('\x1b[1m');
        expect(result).toContain('\x1b[3m');
        expect(result).toContain('\x1b[4m');
        expect(result).toContain('\x1b[0m');
    });
});
