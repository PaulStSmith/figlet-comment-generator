# 🌐 **BYTEFORGE FIGLET SUITE — VISUAL STUDIO EXTENSION**

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

> **FIGLet Comment Generator for Visual Studio**  
> *Generate ASCII art banners directly inside your C#, C++, and .NET projects.*

## 📘 Overview

![Extension Screenshot]

The **FIGLet Comment Generator** extension brings FIGLet ASCII art directly into **Visual Studio**.  
Create bold, readable section headers and file banners inside your code with a single command.

This extension is powered by the **FIGLet .NET Library** — the same engine used across the ByteForge FIGLet Suite — ensuring accurate, spec‑compliant rendering of FIGLet fonts and smushing rules.

## ✨ Features

- 🎨 Generate FIGLet ASCII banners inside Visual Studio  
- 🔤 Choose from multiple FIGLet fonts  
- 🧠 Automatically wraps banners using the correct comment syntax  
- ⚙️ Supports Full Size, Kerning, and Smushing layout modes  
- 🧩 Works with any language that uses line or block comments  
- 🖥️ Clean, native Visual Studio UI  
- 🧱 Powered by the official .NET FIGLet engine  

## 🛠 Installation

### **From Visual Studio Marketplace**
1. Open **Extensions → Manage Extensions**  
2. Search for:  
   **FIGLet Comment Generator**  
3. Click **Download**  
4. Restart Visual Studio to complete installation  

### **Supported Versions**
- Visual Studio **2022**  
- Visual Studio **2026**  

## 🚀 Usage

### **Insert a FIGLet Banner**
1. Open any source-code editor.
2. Right-click where you want the banner to be and select `Insert FIGLet Banner`
3. Enter your text  
4. Choose a font  
5. Press **OK**  

The extension will insert a fully formatted ASCII banner wrapped in the correct comment syntax for your file type.

## 🧩 Supported Languages

The extension automatically detects the file type and applies the correct comment style:

- C# / C++ / Java → `//` or `/* */`  
- F# → `//`  
- VB.NET → `'`  
- SQL → `--`  
- JavaScript / TypeScript → `//`  
- Many others supported through Visual Studio’s language service  

## ⚙️ Settings & Customization

Open **Tools → Options → FIGLet Comment Generator** to configure:

- Default FIGLet font  
- Default layout mode  
- FIGLet font folder

## 🔧 Powered By

This extension is powered by the **FIGLet .NET Library** — the same engine used across the ByteForge FIGLet Suite — ensuring accurate, spec‑compliant rendering of FIGLet fonts and smushing rules.

- Full FLF font parsing  
- All official smushing rules  
- Layout modes (FullSize, Kerning, Smushing)  
- Hardblank handling  
- Accurate horizontal/vertical layout logic  

## 🧱 Example

Input:
```
My Section
```

Output (using “Small” font):
```csharp
/*
 *  __  __          ___         _   _          
 * |  \/  |_  _    / __| ___ __| |_(_)___ _ _  
 * | |\/| | || |   \__ \/ -_) _|  _| / _ \ ' \ 
 * |_|  |_|\_, |   |___/\___\__|\__|_\___/_||_|
 *         |__/                                
 */
```

## 🤝 Contributing

Contributions are welcome!  
To contribute:

1. Fork the repository  
2. Create a feature branch  
3. Commit your changes  
4. Open a Pull Request  

## 📜 License

This extension is licensed under the **MIT License**.

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