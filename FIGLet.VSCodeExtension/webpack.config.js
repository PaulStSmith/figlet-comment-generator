//@ts-check
'use strict';

const path = require('path');

/** @type {import('webpack').Configuration} */
const config = {
    target: 'web',
    mode: 'development',

    entry: './src/webview/index.tsx',
    output: {
        path: path.resolve(__dirname, 'media'),
        filename: 'webview.js'
    },

    resolve: {
        extensions: ['.ts', '.tsx', '.js', '.jsx']
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
                        // Important: use a separate tsconfig for the webview
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

    // These dependencies are provided by VS Code's webview
    externals: {
        vscode: 'commonjs vscode'
    },

    performance: {
        hints: false
    }
};

module.exports = config;