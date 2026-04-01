import { readFileSync } from 'fs';
import { deflateRawSync } from 'zlib';
import { fileURLToPath } from 'url';
import { dirname, join } from 'path';
import { FIGFont } from '../src/FIGFont.js';
import { SmushingRules } from '../src/SmushingRules.js';
import { expect } from 'vitest';

const __dirname = dirname(fileURLToPath(import.meta.url));
const fontsDir = join(__dirname, 'fonts');

// ---------------------------------------------------------------------------
// Font creation helpers
// ---------------------------------------------------------------------------

/**
 * Creates the text content of a minimal valid .flf font with the given
 * height, hard-blank character, and optional print direction.
 * Mirrors C# TestUtilities.CreateMinimalValidFontContent.
 */
export function createMinimalValidFontContent(
    height = 5,
    hardBlank = '$',
    printDirection = 0
): string {
    const lines: string[] = [];
    const dirSuffix = printDirection !== 0 ? ` ${printDirection}` : '';
    lines.push(`flf2a${hardBlank} ${height} 4 10 15 0${dirSuffix}`);

    for (let charCode = 32; charCode <= 126; charCode++) {
        for (let line = 0; line < height; line++) {
            if (charCode === 32) {
                // Space uses the hard-blank character
                lines.push(line === height - 1 ? `${hardBlank}@@` : `${hardBlank}@`);
            } else if (line === height - 1) {
                lines.push(`${String.fromCharCode(charCode)}@@`);
            } else {
                lines.push(`${String.fromCharCode(charCode)}@`);
            }
        }
    }

    return lines.join('\n') + '\n';
}

/**
 * Loads a test font by name from the project's fonts directory.
 * Falls back to mini-fixed if the requested font cannot be loaded.
 * Mirrors C# TestUtilities.LoadTestFont.
 */
export function loadTestFont(fontName: string): FIGFont {
    try {
        const content = readFileSync(join(fontsDir, `${fontName}.flf`), 'latin1');
        const font = FIGFont.fromText(content);
        if (!font) throw new Error(`Failed to parse font: ${fontName}`);
        return font;
    } catch (err) {
        if (fontName !== 'mini-fixed') {
            const content = readFileSync(join(fontsDir, 'mini-fixed.flf'), 'latin1');
            const font = FIGFont.fromText(content);
            if (!font) throw new Error('Failed to load fallback font: mini-fixed');
            return font;
        }
        throw err;
    }
}

// ---------------------------------------------------------------------------
// ZIP helpers (mirrors C# TestUtilities.CreateZipWithFontFile)
// ---------------------------------------------------------------------------

/**
 * Simple CRC-32 implementation (IEEE 802.3 polynomial).
 */
function crc32(buf: Buffer): number {
    let crc = 0xFFFFFFFF;
    for (let i = 0; i < buf.length; i++) {
        crc ^= buf[i];
        for (let j = 0; j < 8; j++) {
            crc = (crc & 1) ? ((crc >>> 1) ^ 0xEDB88320) : (crc >>> 1);
        }
    }
    return (crc ^ 0xFFFFFFFF) >>> 0;
}

/**
 * Creates a minimal ZIP archive buffer containing one font file entry.
 * @param content   The font file content (text).
 * @param fileName  The entry name inside the ZIP.
 * @param store     When true, uses Store (method 0); otherwise Deflate (method 8).
 */
export function createZipWithFontFile(
    content: string,
    fileName = 'test.flf',
    store = false
): Buffer {
    const data = Buffer.from(content, 'latin1');
    const crc = crc32(data);
    const nameBytes = Buffer.from(fileName, 'utf-8');

    const compressed = store ? data : deflateRawSync(data);
    const method = store ? 0 : 8;

    // Local file header (30 bytes) + filename + compressed data
    const header = Buffer.alloc(30);
    header.writeUInt32LE(0x04034B50, 0);            // PK\x03\x04 signature
    header.writeUInt16LE(20, 4);                     // version needed
    header.writeUInt16LE(0, 6);                      // flags
    header.writeUInt16LE(method, 8);                 // compression method
    header.writeUInt16LE(0, 10);                     // mod time
    header.writeUInt16LE(0, 12);                     // mod date
    header.writeUInt32LE(crc, 14);                   // CRC-32
    header.writeUInt32LE(compressed.length, 18);     // compressed size
    header.writeUInt32LE(data.length, 22);           // uncompressed size
    header.writeUInt16LE(nameBytes.length, 26);      // filename length
    header.writeUInt16LE(0, 28);                     // extra field length

    return Buffer.concat([header, nameBytes, compressed]);
}

// ---------------------------------------------------------------------------
// Assertion helpers
// ---------------------------------------------------------------------------

export function assertSmushingRule(
    font: FIGFont,
    rule: SmushingRules,
    shouldHave = true
): void {
    const has = font.hasSmushingRule(rule);
    if (shouldHave) {
        expect(has).toBe(true);
    } else {
        expect(has).toBe(false);
    }
}

export function stripANSIColors(text: string): string {
    return text.replace(/\x1B\[[0-?]*[ -/]*[@-~]/g, '');
}

/**
 * Asserts that two multi-line strings are equal, ignoring carriage-return
 * differences (\r\n vs \n). Mirrors C# TestUtilities.AssertMultiLineEqual.
 */
export function assertMultiLineEqual(expected: string, actual: string, message = ''): void {
    actual   = actual.replace(/\r/g, '');
    expected = expected.replace(/\r/g, '');
    const expectedLines = expected.split('\n');
    const actualLines   = actual.split('\n');
    expect(
        actualLines.length,
        `${message}Line count mismatch. Expected: ${expectedLines.length}, Actual: ${actualLines.length}`
    ).toBe(expectedLines.length);
    for (let i = 0; i < expectedLines.length; i++) {
        expect(
            actualLines[i],
            `${message}Line ${i + 1} mismatch. Expected: '${expectedLines[i]}', Actual: '${actualLines[i]}'`
        ).toBe(expectedLines[i]);
    }
}

/**
 * Asserts that an action completes within a given time budget (milliseconds).
 * Mirrors C# TestUtilities.AssertPerformance.
 */
export function assertPerformance(
    action: () => void,
    maxMs: number,
    operationName = 'Operation'
): void {
    const start = Date.now();
    action();
    const elapsed = Date.now() - start;
    expect(
        elapsed,
        `${operationName} took ${elapsed}ms, but should complete within ${maxMs}ms`
    ).toBeLessThanOrEqual(maxMs);
}

// ---------------------------------------------------------------------------
// Random text generator (fixed seed for reproducibility)
// ---------------------------------------------------------------------------

export function generateLargeText(length: number): string {
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 ';
    // Simple LCG seeded at 42 to match C# Random(42)
    let seed = 42;
    const rand = () => {
        seed = (seed * 1664525 + 1013904223) & 0x7FFFFFFF;
        return seed / 0x7FFFFFFF;
    };
    let result = '';
    for (let i = 0; i < length; i++) {
        result += chars[Math.floor(rand() * chars.length)];
    }
    return result;
}

// ---------------------------------------------------------------------------
// Test data — mirrors C# TestUtilities.TestTexts / ANSIColoredTexts
// (uses "\n" separators to match the TypeScript renderer's default)
// ---------------------------------------------------------------------------

/** Expected substrings that render("key") output should contain. */
export const testTexts: Record<string, string> = {
    'Hello':                  'Helo\nHelo\nHelo\n',
    'World':                  'World\nWorld\nWorld\n',
    'Test':                   'Test\nTest\nTest\n',
    '123':                    '123\n123\n123\n',
    'ABC':                    'ABC\nABC\nABC\n',
    'The quick brown fox':    'The quick brown fox\nThe quick brown fox\nThe quick brown fox\n',
    'a':                      'a\na\na\n',
    'Hello\nWorld':           'Helo\nHelo\nHelo\nWorld\nWorld\nWorld\n',
    'Line1\nLine2\nLine3':    'Line1\nLine1\nLine1\nLine2\nLine2\nLine2\nLine3\nLine3\nLine3\n',
};

/** Expected substrings for ANSI-colored input (renderer.useANSIColors = true). */
export const ansiColoredTexts: Record<string, string> = {
    '\x1b[31mRed\x1b[0m':
        '\x1b[31mRed\x1b[0m\n\x1b[31mRed\x1b[0m\n\x1b[31mRed\x1b[0m\n',
    '\x1b[32mGreen\x1b[33mYellow\x1b[0m':
        '\x1b[32mGren\x1b[33mYelow\x1b[0m\n\x1b[32mGren\x1b[33mYelow\x1b[0m\n\x1b[32mGren\x1b[33mYelow\x1b[0m\n',
    'Normal\x1b[96mCyan\x1b[0mBack':
        'Normal\x1b[96mCyan\x1b[0mBack\x1b[0m\nNormal\x1b[96mCyan\x1b[0mBack\x1b[0m\nNormal\x1b[96mCyan\x1b[0mBack\x1b[0m\n',
};
