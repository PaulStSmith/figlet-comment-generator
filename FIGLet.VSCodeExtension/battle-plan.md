# Battle Plan: Porting FIGlet Comment Generator to VSCode

## 1. Core Components
- [x] FIGlet Engine (Ported to TypeScript)
- [x] Basic Extension Structure
- [x] Language Support (LanguageCommentStyles ported)
- [x] Basic Banner Insertion with Language-Aware Comments

## 2. Implementation Phases

### Phase 1: Basic Extension Setup & Commands ✓
1. [x] Define initial package.json command
   - [x] insertBanner
2. [x] Create basic command handler
3. [x] Implement basic FIGlet insertion
4. [ ] Configure keybindings
5. [ ] Add context menu items
6. [x] Implement configuration settings
   - [x] Font directory with browse button
   - [x] Default font selection from directory
   - [x] Layout mode

### Phase 2: Language Support & Comment Detection ⚡(In Progress)
1. [x] Port LanguageCommentStyles.cs functionality
   - [x] Create language-specific comment mappings
   - [x] Implement comment wrapping logic
2. [x] Create utility functions for:
   - [x] Getting current language ID
   - [x] Determining appropriate comment style
   - [x] Handling indentation
3. [ ] Add configuration options for:
   - [ ] Preferred comment style (block vs line)
   - [ ] Custom comment markers per language

### Phase 3: Code Element Detection
1. [ ] Implement VSCode equivalent of CodeElementDetector
   - [ ] Use VSCode's DocumentSymbolProvider
   - [ ] Create functions to detect classes/methods at cursor
2. [ ] Add support for:
   - [ ] Class detection
   - [ ] Method detection
   - [ ] Proper insertion point detection
3. [ ] Add commands:
   - [ ] insertClassBanner
   - [ ] insertMethodBanner

### Phase 4: UI Implementation ⚡(In Progress)
1. [x] Create QuickPick interface for font selection
2. [ ] Improve input box for banner text
3. [ ] Add preview functionality
   - [ ] Create custom preview panel using WebView
   - [ ] Show live preview as user types
4. [x] Add configuration UI
   - [x] Font directory settings with browse button
   - [x] Font selection QuickPick
   - [ ] Layout mode selection
   - [ ] Preview in settings

### Phase 5: Font Management ⚡(In Progress)
1. [x] Implement font loading system
   - [x] Directory configuration
   - [x] Font enumeration
   - [x] Error handling
2. [ ] Add font caching
3. [ ] Support default FIGlet fonts
4. [x] Add custom font directory support

### Phase 6: Polish & Additional Features
1. [ ] Add error handling and user feedback
2. [ ] Implement progress indicators
3. [ ] Add status bar items
4. [ ] Create comprehensive documentation
   - [ ] README.md
   - [ ] CHANGELOG.md
   - [ ] Examples and screenshots

### Phase 7: Testing & Publishing
1. [ ] Write tests
   - [ ] Unit tests for core functionality
   - [ ] Integration tests for VSCode API usage
2. [ ] Package extension
3. [ ] Create marketplace assets
   - [ ] Icon
   - [ ] Screenshots
   - [ ] Description
4. [ ] Publish to VS Code Marketplace

## 3. Next Immediate Steps

1. Layout Mode Configuration
   - Add layout mode options to settings
   - Implement layout mode handling
   - Add preview capability

2. Context Menu
   - Add editor context menu integration
   - Configure appropriate menu items
   - Add keybindings

3. Code Detection
   - Implement DocumentSymbolProvider
   - Add class/method detection
   - Update insertion point logic

## 4. Current Focus
- Completing font management system
- Adding layout mode configuration
- Beginning work on preview functionality

## 5. Migration Strategy
1. [x] Complete basic functionality (MVP)
2. [x] Add initial configuration and customization
3. [ ] Implement advanced features
4. [ ] Polish and refine user experience
5. [ ] Prepare for marketplace release

## Notes
- Basic FIGlet insertion with language-aware comments is working
- Language detection and comment wrapping implemented
- Font configuration system implemented with browse button and font selection
- Next focus should be on layout mode and preview functionality
- Code element detection will enable more advanced features