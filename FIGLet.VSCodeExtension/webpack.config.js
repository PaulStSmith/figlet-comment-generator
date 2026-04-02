//@ts-check
'use strict';

const path = require('path');

/** @type {import('webpack').Configuration} */
const sharedConfig = {
    target: 'web',
    mode: 'development',

    resolve: {
        extensions: ['.ts', '.tsx', '.js', '.jsx'],
        extensionAlias: {
            '.js': ['.ts', '.js']
        },
        fallback: {
            'fs': false,
            'fs/promises': false,
            'path': false,
            'util': false
        }
    },

    module: {
        rules: [
            {
                test: /\.tsx?$/,
                exclude: /node_modules/,
                use: [{
                    loader: 'ts-loader',
                    options: {
                        configFile: path.resolve(__dirname, 'tsconfig.json'),
                        compilerOptions: {
                            module: 'esnext',
                            moduleResolution: 'node',
                            jsx: 'react'
                        }
                    }
                }]
            }
        ]
    },

    devtool: 'source-map',

    externals: {
        vscode: 'commonjs vscode'
    },

    performance: {
        hints: false
    }
};

module.exports = [
    {
        ...sharedConfig,
        entry: './src/webview/index.tsx',
        output: {
            path: path.resolve(__dirname, 'media'),
            filename: 'webview.js'
        },
    },
    {
        ...sharedConfig,
        entry: './src/webview/settings-index.tsx',
        output: {
            path: path.resolve(__dirname, 'media'),
            filename: 'settings-webview.js'
        },
    },
];
