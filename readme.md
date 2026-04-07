```
██████╗ ██╗   ██╗████████╗███████╗███████╗ ██████╗ ██████╗  ██████╗ ███████╗
██╔══██╗╚██╗ ██╔╝╚══██╔══╝██╔════╝██╔════╝██╔═══██╗██╔══██╗██╔════╝ ██╔════╝
██████╔╝ ╚████╔╝    ██║   █████╗  █████╗  ██║   ██║██████╔╝██║  ███╗█████╗
██╔══██╗  ╚██╔╝     ██║   ██╔══╝  ██╔══╝  ██║   ██║██╔══██╗██║   ██║██╔══╝
██████╔╝   ██║      ██║   ███████╗██║     ╚██████╔╝██║  ██║╚██████╔╝███████╗
╚═════╝    ╚═╝      ╚═╝   ╚══════╝╚═╝      ╚═════╝ ╚═╝  ╚═╝ ╚═════╝ ╚══════╝
                 ███████╗██╗ ██████╗ ██╗     ███████╗████████╗    ███████╗██╗   ██╗██╗████████╗███████╗
                 ██╔════╝██║██╔════╝ ██║     ██╔════╝╚══██╔══╝    ██╔════╝██║   ██║██║╚══██╔══╝██╔════╝
                 █████╗  ██║██║  ███╗██║     █████╗     ██║       ███████╗██║   ██║██║   ██║   █████╗
                 ██╔══╝  ██║██║   ██║██║     ██╔══╝     ██║       ╚════██║██║   ██║██║   ██║   ██╔══╝
                 ██║     ██║╚██████╔╝███████╗███████╗   ██║       ███████║╚██████╔╝██║   ██║   ███████╗
                 ╚═╝     ╚═╝ ╚═════╝ ╚══════╝╚══════╝   ╚═╝       ╚══════╝ ╚═════╝ ╚═╝   ╚═╝   ╚══════╝
```
> **ByteForge FIGLet Suite**
> *A unified ecosystem for generating ASCII art banners across editors, libraries, and terminals.*

---

## 🌐 Overview

The **ByteForge FIGLet Suite** is a cross‑platform ecosystem for creating stylish ASCII art banners using the classic FIGLet specification.
It includes libraries, IDE extensions, and command‑line tools that make it easy to generate and insert FIGLet text anywhere — from code comments to terminal output.

## 🧩 Ecosystem Components

| Component                   | Description                                                                               | Language   | Link                                                                                                         |
| --------------------------- | ----------------------------------------------------------------------------------------- | ---------- | ------------------------------------------------------------------------------------------------------------ |
| **FIGLet .NET Library**     | A .NET implementation of FIGLet. Powers the Visual Studio extension and FigPrint CLI.     | C#         | [NuGet](https://www.nuget.org/packages/FIGLet/#readme-body-tab)                                              |
| **FIGLet TS Library**       | TypeScript implementation for Node.js and web environments. Powers the VS Code extension. | TypeScript | [NPM](https://www.npmjs.com/package/@byte-forge/figlet)                                                      |
| **Visual Studio Extension** | Adds FIGLet banner generation directly into Visual Studio.                                | C#         | [VS Extension](https://marketplace.visualstudio.com/items?itemName=PaulStSmith.FIGLetCommentGenerator)       |
| **VS Code Extension**       | Generate FIGLet banners in VS Code with live preview and comment wrapping.                | TypeScript | [VSCode Extension](https://marketplace.visualstudio.com/items?itemName=PaulStSmith.figlet-comment-generator) |
| **FigPrint CLI**            | Command‑line tool for rendering FIGLet text in the terminal.                              | C#         | `winget install ByteForge.FIGPrint`(*)                                                                       |

(*): A PR for inclusion of _FIGPrint CLI_ into the WinGet library is currently pending review. (see [#355518](https://github.com/microsoft/winget-pkgs/pull/355518))

## 🧱 Architecture

```
                     FIGLet Specification
                           (FLF fonts)
                                │
              ┌─────────────────┴─────────────────┐
              │                                   │
       ┌──────v──────┐                      ┌─────v─────┐
       │ FIGLet .NET │                      │ FIGLet TS │
       │ Library     │                      │ Library   │
       └──────┬──────┘                      └─────┬─────┘
              │                                   │
      ┌───────┴───────┐                  ┌────────┴───────────┐
      │               │                  │                    │
 ┌────v────┐   ┌──────v───────┐  ┌───────v──────┐   ┌─────────v──────────┐
 │ VS Ext. │   │ FigPrint CLI │  │ VS Code Ext. │   │ (future: web/CLI?) │
 └─────────┘   └──────────────┘  └──────────────┘   └────────────────────┘
```

## 🚀 Getting Started

Choose your environment:

- **Visual Studio** → Install *FIGLet Comment Generator* from the Marketplace.
- **VS Code** → Install *FIGLet Comment Generator* from the Marketplace.
- **CLI** → Install *FigPrint*, using `winget install ByteForge.FIGPrint` and run `figprint Hello World`.
- **.NET / Node.js** → Add the FIGLet library via NuGet or npm.

## 🤝 Contributing

Contributions are welcome!
Please open an issue or submit a pull request.

## 📜 License

All projects in the ByteForge FIGLet Suite are licensed under the **MIT License**.

## 💡 Credits

- Original FIGLet concept by **Frank, Ian & Glenn**
- Implementations by **Paulo Santos (ByteForge)**
- FIGLet specifications: [figlet.org](http://www.figlet.org/)

## Support

If you encounter any issues or have feature requests, please:
1. Search existing [issues](https://github.com/PaulStSmith/FIGLet-comment-generator/issues)
2. Create a new issue if needed

---

Made with ❤️ by Paulo Santos
