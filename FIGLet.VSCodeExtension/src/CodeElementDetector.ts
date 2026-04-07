import * as vscode from 'vscode';

/*
 *    ___         _      ___ _               _   ___       _          _
 *   / __|___  __| |___ | __| |___ _ __  ___| |_|   \ ___| |_ ___ __| |_ ___ _ _
 *  | (__/ _ \/ _` / -_)| _|| / -_) '  \/ -_)  _| |) / -_)  _/ -_) _|  _/ _ \ '_|
 *   \___\___/\__,_\___||___|_\___|_|_|_\___|\__|___/\___|\__\___\__|\__\___/_|
 *
 */

/**
 * Represents a detected code element (class, method, etc.) at the cursor position.
 */
export interface CodeElementInfo {
    /** The simple name of the element (e.g. "MyClass" or "doSomething"). */
    name: string;
    /** The 0-based line number where the element's declaration begins. */
    startLine: number;
}

/**
 * Symbol kinds that represent class-like declarations.
 * Mirrors the VSClassLikeElement enum in the VS extension.
 */
export const CLASS_LIKE_KINDS = new Set<vscode.SymbolKind>([
    vscode.SymbolKind.Class,
    vscode.SymbolKind.Interface,
    vscode.SymbolKind.Struct,
    vscode.SymbolKind.Enum,
    vscode.SymbolKind.Module,
]);

/**
 * Symbol kinds that represent method/function-like declarations.
 * Mirrors the VSMemberElement enum in the VS extension.
 */
export const METHOD_LIKE_KINDS = new Set<vscode.SymbolKind>([
    vscode.SymbolKind.Function,
    vscode.SymbolKind.Method,
    vscode.SymbolKind.Constructor,
]);

/**
 * Returns the deepest symbol of one of the specified kinds whose range contains
 * the given position, or `undefined` if none is found.
 *
 * Uses `vscode.executeDocumentSymbolProvider` which relies on the language
 * server for the active file; returns `undefined` gracefully if no provider
 * is available.
 */
export async function findSymbolAtCursor(
    document: vscode.TextDocument,
    position: vscode.Position,
    kinds: Set<vscode.SymbolKind>
): Promise<CodeElementInfo | undefined> {
    let symbols: vscode.DocumentSymbol[];
    try {
        symbols = await vscode.commands.executeCommand<vscode.DocumentSymbol[]>(
            'vscode.executeDocumentSymbolProvider',
            document.uri
        ) ?? [];
    } catch {
        return undefined;
    }
    return _findDeepest(symbols, position, kinds);
}

/**
 * Recursively walks a DocumentSymbol tree to find the innermost symbol of one
 * of the target kinds whose range contains `position`.
 */
function _findDeepest(
    symbols: vscode.DocumentSymbol[],
    position: vscode.Position,
    kinds: Set<vscode.SymbolKind>
): CodeElementInfo | undefined {
    let best: CodeElementInfo | undefined;

    for (const sym of symbols) {
        if (!sym.range.contains(position)) continue;

        // Recurse into children first so inner declarations beat outer ones.
        const fromChild = _findDeepest(sym.children, position, kinds);
        if (fromChild) {
            best = fromChild;
        } else if (kinds.has(sym.kind)) {
            best = { name: sym.name, startLine: sym.range.start.line };
        }
    }

    return best;
}
