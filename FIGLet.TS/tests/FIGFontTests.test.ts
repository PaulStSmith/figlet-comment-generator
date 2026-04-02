import { describe, it, expect, beforeAll } from 'vitest';
import { writeFile, unlink } from 'fs/promises';
import { tmpdir } from 'os';
import { join } from 'path';
import { FIGFont } from '../src/FIGFont.js';
import { SmushingRules } from '../src/SmushingRules.js';
import {
    createMinimalValidFontContent,
    createZipWithFontFile,
    loadTestFont,
    assertSmushingRule,
} from './TestUtilities.js';

// ---------------------------------------------------------------------------
// FIGFont tests — mirrors C# FIGFontTests
// ---------------------------------------------------------------------------

describe('FIGFont', () => {

    // -----------------------------------------------------------------------
    // Default font
    // -----------------------------------------------------------------------

    describe('getDefault', () => {
        it('should load successfully', async () => {
            const font = await FIGFont.getDefault();
            expect(font).not.toBeNull();
            expect(font.signature.startsWith('flf2a')).toBe(true);
            expect(font.height).toBeGreaterThan(0);
            expect(font.characters.size).toBeGreaterThanOrEqual(95);
        });

        it('should be cached — two calls return same properties', async () => {
            const font1 = await FIGFont.getDefault();
            const font2 = await FIGFont.getDefault();
            expect(font1.signature).toBe(font2.signature);
            expect(font1.height).toBe(font2.height);
            expect(font1.hardBlank).toBe(font2.hardBlank);
        });
    });

    // -----------------------------------------------------------------------
    // fromFile
    // -----------------------------------------------------------------------

    describe('fromFile', () => {
        it('should return null for null path', async () => {
            const font = await FIGFont.fromFile(null);
            expect(font).toBeNull();
        });

        it('should return null for empty path', async () => {
            const font = await FIGFont.fromFile('');
            expect(font).toBeNull();
        });

        it('should return null for non-existent file', async () => {
            // TypeScript fromFile catches I/O errors and returns null
            // (unlike C# which throws FileNotFoundException)
            const font = await FIGFont.fromFile('nonexistent.flf');
            expect(font).toBeNull();
        });
    });

    // -----------------------------------------------------------------------
    // fromText
    // -----------------------------------------------------------------------

    describe('fromText', () => {
        it('should return null for null input', () => {
            const font = FIGFont.fromText(null);
            expect(font).toBeNull();
        });

        it('should return null for empty string', () => {
            const font = FIGFont.fromText('');
            expect(font).toBeNull();
        });
    });

    // -----------------------------------------------------------------------
    // fromLines
    // -----------------------------------------------------------------------

    describe('fromLines', () => {
        it('should return null for null array', () => {
            const font = FIGFont.fromLines(null);
            expect(font).toBeNull();
        });

        it('should return null for empty array', () => {
            const font = FIGFont.fromLines([]);
            expect(font).toBeNull();
        });

        it('should throw for invalid signature', () => {
            const lines = ['invalid_signature 5 4 10 15 0'];
            expect(() => FIGFont.fromLines(lines)).toThrow('Invalid FIGfont format');
        });

        it('should parse a valid minimal font', () => {
            const content = createMinimalValidFontContent();
            const lines = content.split('\n');
            const font = FIGFont.fromLines(lines);

            expect(font).not.toBeNull();
            expect(font!.signature).toBe('flf2a$');
            expect(font!.hardBlank).toBe('$');
            expect(font!.height).toBe(5);
            expect(font!.baseline).toBe(4);
            expect(font!.maxLength).toBe(10);
            expect(font!.oldLayout).toBe(15);
            expect(font!.characters.size).toBe(95); // ASCII 32-126
        });
    });

    // -----------------------------------------------------------------------
    // fromText — round-trip through minimal content
    // -----------------------------------------------------------------------

    it('fromText should parse a minimal font correctly', () => {
        const content = createMinimalValidFontContent(3, '#');
        const font = FIGFont.fromText(content);

        expect(font).not.toBeNull();
        expect(font!.hardBlank).toBe('#');
        expect(font!.height).toBe(3);
    });

    // -----------------------------------------------------------------------
    // Test font loading
    // -----------------------------------------------------------------------

    it('loadTestFont(mini-fixed) should load correctly', () => {
        const font = loadTestFont('mini-fixed');
        expect(font).not.toBeNull();
        expect(font.signature).toBe('flf2a$');
        expect(font.height).toBe(3);
        expect(font.characters.size).toBe(95);
    });

    // -----------------------------------------------------------------------
    // Smushing rules
    // -----------------------------------------------------------------------

    it('smushing-test font should have all smushing rules', () => {
        const font = loadTestFont('smushing-test');
        assertSmushingRule(font, SmushingRules.EqualCharacter, true);
        assertSmushingRule(font, SmushingRules.Underscore, true);
        assertSmushingRule(font, SmushingRules.Hierarchy, true);
        assertSmushingRule(font, SmushingRules.OppositePair, true);
        assertSmushingRule(font, SmushingRules.BigX, true);
        assertSmushingRule(font, SmushingRules.HardBlank, true);
    });

    // -----------------------------------------------------------------------
    // Layout parameter parsing
    // -----------------------------------------------------------------------

    it('old layout -1 should produce no smushing rules', () => {
        const content = createMinimalValidFontContent(2, '$');
        const lines = content.split('\n');
        lines[0] = 'flf2a$ 2 1 5 -1 0';
        const font = FIGFont.fromLines(lines);
        expect(font).not.toBeNull();
        expect(font!.smushingRules).toBe(SmushingRules.None);
    });

    it('old layout 0 should produce no smushing rules', () => {
        const content = createMinimalValidFontContent(2, '$');
        const lines = content.split('\n');
        lines[0] = 'flf2a$ 2 1 5 0 0';
        const font = FIGFont.fromLines(lines);
        expect(font).not.toBeNull();
        expect(font!.smushingRules).toBe(SmushingRules.None);
    });

    it('full layout should take precedence over old layout', () => {
        const content = createMinimalValidFontContent(2, '$');
        const lines = content.split('\n');
        lines[0] = 'flf2a$ 2 1 5 99 0 0 7'; // old=99, full=7
        const font = FIGFont.fromLines(lines);
        expect(font).not.toBeNull();
        expect(font!.fullLayout).toBe(7);
        // full=7 → bit0=1 (smushing enabled), bits1-2=3 → EqualCharacter + Underscore
        assertSmushingRule(font!, SmushingRules.EqualCharacter, true);
        assertSmushingRule(font!, SmushingRules.Underscore, true);
        assertSmushingRule(font!, SmushingRules.Hierarchy, false);
    });

    it('full layout with bit-0 clear should disable all rules', () => {
        const content = createMinimalValidFontContent(2, '$');
        const lines = content.split('\n');
        lines[0] = 'flf2a$ 2 1 5 0 0 0 126'; // full layout = 126 (bit0 = 0)
        const font = FIGFont.fromLines(lines);
        expect(font).not.toBeNull();
        expect(font!.smushingRules).toBe(SmushingRules.None);
    });

    // -----------------------------------------------------------------------
    // Character map completeness
    // -----------------------------------------------------------------------

    it('default font should contain all printable ASCII characters', async () => {
        const font = await FIGFont.getDefault();
        for (let i = 32; i <= 126; i++) {
            const ch = String.fromCodePoint(i);
            expect(font.characters.has(ch)).toBe(true);
            expect(font.characters.get(ch)!.length).toBe(font.height);
        }
    });

    // -----------------------------------------------------------------------
    // Font parsing — comments, printDirection, extended characters
    // -----------------------------------------------------------------------

    it('should parse comments correctly', () => {
        const content = createMinimalValidFontContent(2, '$');
        const lines = content.split('\n');
        lines[0] = 'flf2a$ 2 1 5 0 2'; // 2 comment lines
        lines.splice(1, 0, 'Comment Line 1', 'Comment Line 2');
        const font = FIGFont.fromLines(lines);
        expect(font).not.toBeNull();
        expect(font!.comments).toBe('Comment Line 1\nComment Line 2');
    });

    it('should parse printDirection correctly', () => {
        const content = createMinimalValidFontContent(2, '$', 1);
        const font = FIGFont.fromText(content);
        expect(font).not.toBeNull();
        expect(font!.printDirection).toBe(1);
    });

    it('should parse extended Unicode characters', () => {
        const base = createMinimalValidFontContent(2);
        const extended = base + '8364\nE@\nu@@\n'; // Euro symbol U+20AC = 8364
        const font = FIGFont.fromLines(extended.split('\n'));
        expect(font).not.toBeNull();
        expect(font!.characters.has(String.fromCodePoint(8364))).toBe(true);
        expect(font!.characters.get(String.fromCodePoint(8364))!.length).toBe(2);
    });

    it('should stop at malformed extended character code', () => {
        const base = createMinimalValidFontContent(2);
        const extended = base + 'invalid_code\nE@\nu@@\n';
        const font = FIGFont.fromLines(extended.split('\n'));
        expect(font).not.toBeNull();
        expect(font!.characters.size).toBe(95); // only the 95 standard ASCII chars
    });

    // -----------------------------------------------------------------------
    // Hard-blank variations
    // -----------------------------------------------------------------------

    it('should parse a font with @ as hard-blank character', () => {
        const content = createMinimalValidFontContent(2, '@');
        const font = FIGFont.fromText(content);
        expect(font).not.toBeNull();
        expect(font!.hardBlank).toBe('@');
    });

    // -----------------------------------------------------------------------
    // ZIP loading via fromFile
    // -----------------------------------------------------------------------

    it('fromFile should load a ZIP (Deflate) font correctly', async () => {
        const fontContent = createMinimalValidFontContent(2);
        const zipBytes = createZipWithFontFile(fontContent, 'test.flf', false);
        const tempFile = join(tmpdir(), `figlet_test_deflate_${Date.now()}.zip`);
        await writeFile(tempFile, zipBytes);
        try {
            const font = await FIGFont.fromFile(tempFile);
            expect(font).not.toBeNull();
            expect(font!.height).toBe(2);
            expect(font!.characters.size).toBe(95);
        } finally {
            await unlink(tempFile).catch(() => {});
        }
    });

    it('fromFile should load a ZIP (Store) font correctly', async () => {
        const fontContent = createMinimalValidFontContent(3, '@');
        const zipBytes = createZipWithFontFile(fontContent, 'test.flf', true);
        const tempFile = join(tmpdir(), `figlet_test_store_${Date.now()}.zip`);
        await writeFile(tempFile, zipBytes);
        try {
            const font = await FIGFont.fromFile(tempFile);
            expect(font).not.toBeNull();
            expect(font!.hardBlank).toBe('@');
            expect(font!.height).toBe(3);
        } finally {
            await unlink(tempFile).catch(() => {});
        }
    });

    it('fromFile with invalid ZIP bytes should return null', async () => {
        // "PK" magic but invalid local-file header — fromFile catches and returns null
        const fakeZip = 'PK' + createMinimalValidFontContent(2);
        const tempFile = join(tmpdir(), `figlet_test_invalid_zip_${Date.now()}.zip`);
        await writeFile(tempFile, fakeZip);
        try {
            const font = await FIGFont.fromFile(tempFile);
            expect(font).toBeNull();
        } finally {
            await unlink(tempFile).catch(() => {});
        }
    });
});
