import React, { useState, useEffect, useMemo } from 'react';
import { FIGFont } from '../FIGLet/FIGFont';
import { FIGLetRenderer } from '../FIGLet/FIGLetRenderer';
import { LayoutMode } from '../FIGLet/LayoutMode';
import { getVsCodeApi } from './vscodeApi';

type LayoutKey = 'full' | 'kerning' | 'smush';

interface FontRow {
    name: string;
    height: number;
    baseline: number;
    maxLength: number;
    smushingRules: number;
    content: string;
}

interface Settings {
    fontDirectory: string;
    defaultFont: string;
    layoutMode: LayoutKey;
}

function smushingRulesToString(rules: number): string {
    if (rules === 0) { return 'None'; }
    const parts: string[] = [];
    if (rules & 1)  { parts.push('Equal'); }
    if (rules & 2)  { parts.push('Underscore'); }
    if (rules & 4)  { parts.push('Hierarchy'); }
    if (rules & 8)  { parts.push('OppositePair'); }
    if (rules & 16) { parts.push('BigX'); }
    if (rules & 32) { parts.push('HardBlank'); }
    return parts.join(', ');
}

function toLayoutMode(key: LayoutKey): LayoutMode {
    switch (key) {
        case 'full':    return LayoutMode.FullSize;
        case 'kerning': return LayoutMode.Kerning;
        default:        return LayoutMode.Smushing;
    }
}

const LAYOUT_OPTIONS: Array<{ value: LayoutKey; label: string }> = [
    { value: 'full',    label: 'Full Size' },
    { value: 'kerning', label: 'Kerning'   },
    { value: 'smush',   label: 'Smushing'  },
];

// Inline styles using VS Code CSS variables for automatic theme support
const S = {
    container: {
        display: 'flex', flexDirection: 'column' as const, gap: '12px',
        padding: '16px', height: '100vh', boxSizing: 'border-box' as const,
    },
    section: {
        display: 'flex', flexDirection: 'column' as const, gap: '6px',
    },
    sectionTitle: {
        color: 'var(--vscode-foreground)',
        fontSize: 'var(--vscode-font-size)',
        fontWeight: 'bold' as const,
        marginBottom: '2px',
    },
    row: {
        display: 'flex', gap: '8px', alignItems: 'center',
    },
    label: {
        color: 'var(--vscode-foreground)',
        fontSize: 'var(--vscode-font-size)',
        whiteSpace: 'nowrap' as const,
        minWidth: '100px',
    },
    input: {
        background: 'var(--vscode-input-background)',
        color: 'var(--vscode-input-foreground)',
        border: '1px solid var(--vscode-input-border, var(--vscode-dropdown-border))',
        padding: '4px 8px',
        fontSize: 'var(--vscode-font-size)',
        borderRadius: '2px',
        flex: 1,
        outline: 'none',
    },
    select: {
        background: 'var(--vscode-dropdown-background)',
        color: 'var(--vscode-dropdown-foreground)',
        border: '1px solid var(--vscode-dropdown-border)',
        padding: '3px 6px',
        fontSize: 'var(--vscode-font-size)',
        borderRadius: '2px',
        cursor: 'pointer',
        minWidth: '140px',
    },
    btn: {
        background: 'var(--vscode-button-background)',
        color: 'var(--vscode-button-foreground)',
        border: 'none',
        padding: '4px 12px',
        fontSize: 'var(--vscode-font-size)',
        borderRadius: '2px',
        cursor: 'pointer',
        whiteSpace: 'nowrap' as const,
    },
    table: {
        width: '100%',
        borderCollapse: 'collapse' as const,
        fontSize: 'var(--vscode-font-size)',
    },
    th: {
        background: 'var(--vscode-editor-lineHighlightBackground, var(--vscode-editorWidget-border))',
        color: 'var(--vscode-foreground)',
        padding: '4px 8px',
        textAlign: 'left' as const,
        borderBottom: '1px solid var(--vscode-editorWidget-border, var(--vscode-dropdown-border))',
        fontWeight: 'bold' as const,
        cursor: 'pointer',
        userSelect: 'none' as const,
    },
    td: {
        color: 'var(--vscode-foreground)',
        padding: '3px 8px',
        borderBottom: '1px solid var(--vscode-editorWidget-border, var(--vscode-dropdown-border))',
        cursor: 'pointer',
    },
    tdSelected: {
        color: 'var(--vscode-list-activeSelectionForeground)',
        background: 'var(--vscode-list-activeSelectionBackground)',
        padding: '3px 8px',
        borderBottom: '1px solid var(--vscode-editorWidget-border, var(--vscode-dropdown-border))',
        cursor: 'pointer',
    },
    tableContainer: {
        flex: 1,
        overflow: 'auto',
        border: '1px solid var(--vscode-editorWidget-border, var(--vscode-dropdown-border))',
        minHeight: '120px',
        maxHeight: '220px',
    },
    preview: {
        background: 'var(--vscode-editor-background)',
        color: 'var(--vscode-editor-foreground)',
        border: '1px solid var(--vscode-editorWidget-border, var(--vscode-dropdown-border))',
        padding: '8px 12px',
        fontFamily: 'var(--vscode-editor-font-family, monospace)',
        fontSize: 'var(--vscode-editor-font-size, 12px)',
        overflow: 'auto',
        whiteSpace: 'pre' as const,
        borderRadius: '2px',
        minHeight: '60px',
        maxHeight: '160px',
        flex: 1,
    },
    footer: {
        display: 'flex', justifyContent: 'flex-end', gap: '8px',
    },
    btnSecondary: {
        background: 'var(--vscode-button-secondaryBackground)',
        color: 'var(--vscode-button-secondaryForeground)',
        border: 'none',
        padding: '4px 12px',
        fontSize: 'var(--vscode-font-size)',
        borderRadius: '2px',
        cursor: 'pointer',
        minWidth: '72px',
    },
    saved: {
        color: 'var(--vscode-notificationsInfoIcon-foreground, var(--vscode-textLink-foreground))',
        fontSize: 'var(--vscode-font-size)',
        marginRight: '8px',
    },
};

export function SettingsApp() {
    const [fontDirectory, setFontDirectory]   = useState('');
    const [defaultFont,   setDefaultFont]     = useState('small');
    const [layoutMode,    setLayoutMode]       = useState<LayoutKey>('smush');
    const [fonts,         setFonts]            = useState<FontRow[]>([]);
    const [selectedFont,  setSelectedFont]     = useState('small');
    const [previewText,   setPreviewText]      = useState('Preview');
    const [savedMsg,      setSavedMsg]         = useState('');
    const [sortCol,       setSortCol]          = useState<keyof FontRow>('name');
    const [sortAsc,       setSortAsc]          = useState(true);

    const fontCache = useMemo(() => {
        const map = new Map<string, FIGFont>();
        for (const f of fonts) {
            try { map.set(f.name, FIGFont.fromText(f.content)); } catch { /* skip */ }
        }
        return map;
    }, [fonts]);

    const preview = useMemo(() => {
        const font = fontCache.get(selectedFont);
        if (!font)           { return '(font not loaded)'; }
        if (!previewText.trim()) { return ''; }
        try {
            return new FIGLetRenderer(font).render(previewText.trim(), toLayoutMode(layoutMode));
        } catch (e) {
            return `Render error: ${e}`;
        }
    }, [previewText, selectedFont, fontCache, layoutMode]);

    const sortedFonts = useMemo(() => {
        const col = sortCol;
        return [...fonts].sort((a: FontRow, b: FontRow) => {
            const av = a[col];
            const bv = b[col];
            const cmp = typeof av === 'string' ? (av as string).localeCompare(bv as string)
                                               : (av as number) - (bv as number);
            return sortAsc ? cmp : -cmp;
        });
    }, [fonts, sortCol, sortAsc]);

    useEffect(() => {
        const onMessage = (event: MessageEvent) => {
            const msg = event.data;
            if (msg.type === 'init') {
                const s: Settings = msg.settings;
                setFontDirectory(s.fontDirectory || '');
                setDefaultFont(s.defaultFont || 'small');
                setLayoutMode((s.layoutMode as LayoutKey) || 'smush');
                setSelectedFont(s.defaultFont || 'small');
                const rows: FontRow[] = (msg.fonts as Array<{ name: string; content: string }>).map(f => {
                    try {
                        const fig = FIGFont.fromText(f.content);
                        return {
                            name:         f.name,
                            height:       fig.height,
                            baseline:     fig.baseline,
                            maxLength:    fig.maxLength,
                            smushingRules: fig.smushingRules as number,
                            content:      f.content,
                        };
                    } catch {
                        return { name: f.name, height: 0, baseline: 0, maxLength: 0, smushingRules: 0, content: f.content };
                    }
                });
                setFonts(rows);
            } else if (msg.type === 'fontDirectoryUpdated') {
                setFontDirectory(msg.directory || '');
                if (msg.fonts) {
                    const rows: FontRow[] = (msg.fonts as Array<{ name: string; content: string }>).map(f => {
                        try {
                            const fig = FIGFont.fromText(f.content);
                            return { name: f.name, height: fig.height, baseline: fig.baseline, maxLength: fig.maxLength, smushingRules: fig.smushingRules as number, content: f.content };
                        } catch {
                            return { name: f.name, height: 0, baseline: 0, maxLength: 0, smushingRules: 0, content: f.content };
                        }
                    });
                    setFonts(rows);
                }
            }
        };
        window.addEventListener('message', onMessage);
        getVsCodeApi().postMessage({ type: 'ready' });
        return () => window.removeEventListener('message', onMessage);
    }, []);

    const handleBrowse = () => {
        getVsCodeApi().postMessage({ type: 'browseFontDir' });
    };

    const handleSave = () => {
        getVsCodeApi().postMessage({
            type: 'saveSettings',
            settings: { fontDirectory, defaultFont, layoutMode },
        });
        setSavedMsg('Saved.');
        setTimeout(() => setSavedMsg(''), 2000);
    };

    const handleClose = () => {
        getVsCodeApi().postMessage({ type: 'close' });
    };

    const handleFontRowClick = (name: string) => {
        setSelectedFont(name);
        setDefaultFont(name);
    };

    const handleSort = (col: keyof FontRow) => {
        if (col === 'content') { return; }
        if (sortCol === col) { setSortAsc(a => !a); }
        else { setSortCol(col); setSortAsc(true); }
    };

    const sortIndicator = (col: keyof FontRow) =>
        sortCol === col ? (sortAsc ? ' ▲' : ' ▼') : '';

    return (
        <div style={S.container}>
            {/* Font Directory */}
            <div style={S.section}>
                <div style={S.sectionTitle}>Font Directory</div>
                <div style={S.row}>
                    <input
                        type="text"
                        style={S.input}
                        value={fontDirectory}
                        onChange={e => setFontDirectory(e.target.value)}
                        placeholder="(using built-in fonts)"
                        readOnly
                    />
                    <button style={S.btn} onClick={handleBrowse}>Browse...</button>
                </div>
            </div>

            {/* Layout Mode */}
            <div style={S.section}>
                <div style={S.sectionTitle}>Layout Mode</div>
                <div style={S.row}>
                    <select style={S.select} value={layoutMode} onChange={e => setLayoutMode(e.target.value as LayoutKey)}>
                        {LAYOUT_OPTIONS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
                    </select>
                </div>
            </div>

            {/* Font Table */}
            <div style={{ ...S.section, flex: 1, minHeight: 0 }}>
                <div style={S.sectionTitle}>Available Fonts</div>
                <div style={S.tableContainer}>
                    <table style={S.table}>
                        <thead>
                            <tr>
                                <th style={S.th} onClick={() => handleSort('name')}>Name{sortIndicator('name')}</th>
                                <th style={S.th} onClick={() => handleSort('height')}>Height{sortIndicator('height')}</th>
                                <th style={S.th} onClick={() => handleSort('baseline')}>Baseline{sortIndicator('baseline')}</th>
                                <th style={S.th} onClick={() => handleSort('maxLength')}>Max Width{sortIndicator('maxLength')}</th>
                                <th style={S.th} onClick={() => handleSort('smushingRules')}>Layout Rules{sortIndicator('smushingRules')}</th>
                            </tr>
                        </thead>
                        <tbody>
                            {sortedFonts.map(f => {
                                const isSelected = f.name === selectedFont;
                                const tdStyle = isSelected ? S.tdSelected : S.td;
                                return (
                                    <tr key={f.name} onClick={() => handleFontRowClick(f.name)}>
                                        <td style={tdStyle}>{f.name}</td>
                                        <td style={tdStyle}>{f.height}</td>
                                        <td style={tdStyle}>{f.baseline}</td>
                                        <td style={tdStyle}>{f.maxLength}</td>
                                        <td style={tdStyle}>{smushingRulesToString(f.smushingRules)}</td>
                                    </tr>
                                );
                            })}
                        </tbody>
                    </table>
                </div>
            </div>

            {/* Preview */}
            <div style={S.section}>
                <div style={S.sectionTitle}>Preview — {selectedFont}</div>
                <div style={S.row}>
                    <input
                        type="text"
                        style={S.input}
                        value={previewText}
                        onChange={e => setPreviewText(e.target.value)}
                        placeholder="Preview text"
                    />
                </div>
                <pre style={S.preview}>{preview || ' '}</pre>
            </div>

            {/* Footer */}
            <div style={S.footer}>
                {savedMsg && <span style={S.saved}>{savedMsg}</span>}
                <button style={S.btn} onClick={handleSave}>Save</button>
                <button style={S.btnSecondary} onClick={handleClose}>Close</button>
            </div>
        </div>
    );
}
