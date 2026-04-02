import React, { useState, useEffect, useMemo, useRef } from 'react';
import { FIGFont } from '../FIGLet/FIGFont';
import { FIGLetRenderer } from '../FIGLet/FIGLetRenderer';
import { LayoutMode } from '../FIGLet/LayoutMode';
import { LanguageCommentStyles } from '../LanguageCommentStyles';
import { getVsCodeApi } from './vscodeApi';

type LayoutKey = 'full' | 'kerning' | 'smush';

const LAYOUT_OPTIONS: Array<{ value: LayoutKey; label: string }> = [
    { value: 'full',    label: 'Full Size' },
    { value: 'kerning', label: 'Kerning'   },
    { value: 'smush',   label: 'Smushing'  },
];

const LANGUAGES: Array<{ id: string; label: string }> = [
    { id: 'aspx',        label: 'ASP.NET'      },
    { id: 'bash',        label: 'Bash'         },
    { id: 'bat',         label: 'Batch'        },
    { id: 'cpp',         label: 'C/C++'        },
    { id: 'csharp',      label: 'C#'           },
    { id: 'css',         label: 'CSS'          },
    { id: 'd',           label: 'D'            },
    { id: 'fish',        label: 'Fish'         },
    { id: 'fortran',     label: 'Fortran'      },
    { id: 'fsharp',      label: 'F#'           },
    { id: 'go',          label: 'Go'           },
    { id: 'html',        label: 'HTML'         },
    { id: 'java',        label: 'Java'         },
    { id: 'javascript',  label: 'JavaScript'   },
    { id: 'kotlin',      label: 'Kotlin'       },
    { id: 'lisp',        label: 'Lisp'         },
    { id: 'mysql',       label: 'MySQL'        },
    { id: 'objective-c', label: 'Objective-C'  },
    { id: 'pascal',      label: 'Pascal'       },
    { id: 'perl',        label: 'Perl'         },
    { id: 'pgsql',       label: 'PostgreSQL'   },
    { id: 'php',         label: 'PHP'          },
    { id: 'plsql',       label: 'PL/SQL'       },
    { id: 'powershell',  label: 'PowerShell'   },
    { id: 'python',      label: 'Python'       },
    { id: 'r',           label: 'R'            },
    { id: 'ruby',        label: 'Ruby'         },
    { id: 'rust',        label: 'Rust'         },
    { id: 'scala',       label: 'Scala'        },
    { id: 'scheme',      label: 'Scheme'       },
    { id: 'sh',          label: 'Shell'        },
    { id: 'sql',         label: 'SQL'          },
    { id: 'svg',         label: 'SVG'          },
    { id: 'swift',       label: 'Swift'        },
    { id: 'tsql',        label: 'T-SQL'        },
    { id: 'typescript',  label: 'TypeScript'   },
    { id: 'vb',          label: 'Visual Basic' },
    { id: 'xaml',        label: 'XAML'         },
    { id: 'xml',         label: 'XML'          },
    { id: 'yaml',        label: 'YAML'         },
].sort((a, b) => a.label.localeCompare(b.label));

function toLayoutMode(key: LayoutKey): LayoutMode {
    switch (key) {
        case 'full':    return LayoutMode.FullSize;
        case 'kerning': return LayoutMode.Kerning;
        default:        return LayoutMode.Smushing;
    }
}

const PREVIEW_PLACEHOLDER = 'Hello, World!';

function buildPreview(text: string, font: FIGFont | undefined, layout: LayoutKey, language: string): string {
    if (!font) { return '(font not loaded)'; }
    const renderText = text.trim() || PREVIEW_PLACEHOLDER;
    try {
        const rendered = new FIGLetRenderer(font).render(renderText, toLayoutMode(layout));
        return LanguageCommentStyles.wrapInComments(rendered, language);
    } catch (e) {
        return `Render error: ${e}`;
    }
}

// Inline styles using VS Code CSS variables for automatic theme support
const S = {
    container: {
        display: 'flex', flexDirection: 'column' as const, gap: '10px',
        padding: '16px', height: '100vh', boxSizing: 'border-box' as const,
    },
    row: {
        display: 'flex', gap: '12px', alignItems: 'center', flexWrap: 'wrap' as const,
    },
    label: {
        color: 'var(--vscode-foreground)',
        fontSize: 'var(--vscode-font-size)',
        whiteSpace: 'nowrap' as const,
    },
    select: {
        background: 'var(--vscode-dropdown-background)',
        color: 'var(--vscode-dropdown-foreground)',
        border: '1px solid var(--vscode-dropdown-border)',
        padding: '3px 6px',
        fontSize: 'var(--vscode-font-size)',
        borderRadius: '2px',
        cursor: 'pointer',
        minWidth: '120px',
    },
    input: {
        background: 'var(--vscode-input-background)',
        color: 'var(--vscode-input-foreground)',
        border: '1px solid var(--vscode-input-border, var(--vscode-dropdown-border))',
        padding: '4px 8px',
        fontSize: 'var(--vscode-font-size)',
        borderRadius: '2px',
        width: '100%',
        boxSizing: 'border-box' as const,
        outline: 'none',
    },
    preview: {
        flex: 1,
        background: 'var(--vscode-editor-background)',
        color: 'var(--vscode-editor-foreground)',
        border: '1px solid var(--vscode-editorWidget-border, var(--vscode-dropdown-border))',
        padding: '8px 12px',
        fontFamily: 'var(--vscode-editor-font-family, monospace)',
        fontSize: 'var(--vscode-editor-font-size, 12px)',
        overflow: 'auto',
        whiteSpace: 'pre' as const,
        borderRadius: '2px',
        margin: 0,
        minHeight: '80px',
    },
    footer: {
        display: 'flex', alignItems: 'center', gap: '8px',
    },
    spacer: { flex: 1 },
    link: {
        color: 'var(--vscode-textLink-foreground)',
        cursor: 'pointer',
        textDecoration: 'none',
        fontSize: 'var(--vscode-font-size)',
    },
    btn: {
        background: 'var(--vscode-button-background)',
        color: 'var(--vscode-button-foreground)',
        border: 'none',
        padding: '5px 16px',
        fontSize: 'var(--vscode-font-size)',
        borderRadius: '2px',
        cursor: 'pointer',
        minWidth: '72px',
    },
    btnSecondary: {
        background: 'var(--vscode-button-secondaryBackground)',
        color: 'var(--vscode-button-secondaryForeground)',
        border: 'none',
        padding: '5px 16px',
        fontSize: 'var(--vscode-font-size)',
        borderRadius: '2px',
        cursor: 'pointer',
        minWidth: '72px',
    },
};

export function App() {
    const [fontNames,    setFontNames]    = useState<string[]>([]);
    const [selectedFont, setSelectedFont] = useState('small');
    const [fontCache,    setFontCache]    = useState<Map<string, FIGFont>>(new Map());
    const [layout,       setLayout]       = useState<LayoutKey>('smush');
    const [language,     setLanguage]     = useState('csharp');
    const [inputText,    setInputText]    = useState('');
    const inputRef = useRef<HTMLInputElement>(null);

    const preview = useMemo(
        () => buildPreview(inputText, fontCache.get(selectedFont), layout, language),
        [inputText, selectedFont, fontCache, layout, language]
    );

    // Listen for messages from the extension host.
    // Send 'ready' immediately after registering the listener so the extension
    // knows it's safe to postMessage — avoids the race where init arrives before
    // window.addEventListener is called.
    useEffect(() => {
        const onMessage = (event: MessageEvent) => {
            const msg = event.data;
            if (msg.type === 'init') {
                const map = new Map<string, FIGFont>();
                for (const { name, content } of (msg.fonts as Array<{ name: string; content: string }>)) {
                    try { map.set(name, FIGFont.fromText(content)); } catch { /* skip bad font */ }
                }
                const names = Array.from(map.keys());
                setFontCache(map);
                setFontNames(names);

                const def: string = msg.defaultFont && map.has(msg.defaultFont) ? msg.defaultFont : (names[0] ?? 'small');
                setSelectedFont(def);
                if (msg.defaultLayout) { setLayout(msg.defaultLayout as LayoutKey); }
                if (msg.language)      { setLanguage(msg.language); }
            } else if (msg.type === 'fontLoaded') {
                try {
                    const font = FIGFont.fromText(msg.content as string);
                    setFontCache(prev => new Map(prev).set(msg.name as string, font));
                } catch { /* skip */ }
            }
        };
        window.addEventListener('message', onMessage);
        getVsCodeApi().postMessage({ type: 'ready' });
        return () => window.removeEventListener('message', onMessage);
    }, []); // register once

    // Request missing font when selection changes
    useEffect(() => {
        if (selectedFont && !fontCache.has(selectedFont)) {
            getVsCodeApi().postMessage({ type: 'requestFont', name: selectedFont });
        }
    }, [selectedFont, fontCache]);

    // Auto-focus input when fonts are loaded
    useEffect(() => {
        if (fontNames.length > 0) { inputRef.current?.focus(); }
    }, [fontNames]);

    const vscode = getVsCodeApi();

    const handleOk = () => {
        if (!inputText.trim()) { return; }
        vscode.postMessage({ type: 'ok', text: inputText.trim(), font: selectedFont, layoutMode: layout, language });
    };

    const handleCancel = () => {
        vscode.postMessage({ type: 'cancel' });
    };

    const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
        if (e.key === 'Enter')  { handleOk(); }
        if (e.key === 'Escape') { handleCancel(); }
    };

    const handleLinkClick = () => {
        vscode.postMessage({ type: 'openExternal', url: 'https://marketplace.visualstudio.com/items?itemName=PaulStSmith.FIGLetCommentGenerator' });
    };

    return (
        <div style={S.container}>
            {/* Font + Layout */}
            <div style={S.row}>
                <span style={S.label}>Font:</span>
                <select style={S.select} value={selectedFont} onChange={e => setSelectedFont(e.target.value)}>
                    {fontNames.map(n => <option key={n} value={n}>{n}</option>)}
                </select>
                <span style={{ ...S.label, marginLeft: '8px' }}>Layout:</span>
                <select style={S.select} value={layout} onChange={e => setLayout(e.target.value as LayoutKey)}>
                    {LAYOUT_OPTIONS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
                </select>
            </div>

            {/* Text input */}
            <div>
                <div style={{ ...S.label, marginBottom: '4px' }}>Enter text to convert:</div>
                <input
                    ref={inputRef}
                    type="text"
                    style={S.input}
                    value={inputText}
                    onChange={e => setInputText(e.target.value)}
                    onKeyDown={handleKeyDown}
                    placeholder="Your banner text here"
                />
            </div>

            {/* Preview */}
            <pre style={S.preview}>{preview || ' '}</pre>

            {/* Language */}
            <div style={S.row}>
                <span style={S.label}>Language:</span>
                <select style={S.select} value={language} onChange={e => setLanguage(e.target.value)}>
                    {LANGUAGES.map(l => <option key={l.id} value={l.id}>{l.label}</option>)}
                </select>
            </div>

            {/* Footer */}
            <div style={S.footer}>
                <span
                    style={S.link}
                    onClick={handleLinkClick}
                    role="link"
                    tabIndex={0}
                    onKeyDown={e => { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); handleLinkClick(); } }}
                >
                    FIGLet Comment Generator
                </span>
                <div style={S.spacer} />
                <button style={S.btn} onClick={handleOk} disabled={!inputText.trim()}>
                    OK
                </button>
                <button style={S.btnSecondary} onClick={handleCancel}>
                    Cancel
                </button>
            </div>
        </div>
    );
}
