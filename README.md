# Raaya TypeSwitch

**English** | [فارسی](README.fa.md)

**Smart Persian ↔ English keyboard-layout correction for Windows.**

Raaya TypeSwitch is a lightweight, local-first Windows utility that detects text typed with the wrong keyboard layout, fixes the already typed word, and switches the active keyboard layout automatically.

> **Status:** Early MVP / experimental. The current release focuses on Persian ↔ English (US) and performs automatic correction when a word is completed with Space, Enter, or Tab.

## Example

When the active keyboard layout is English but you intend to type Persian:

```text
sghl
```

Raaya TypeSwitch reconstructs the same physical keystrokes using the Persian layout:

```text
سلام
```

If the confidence score is high enough, it replaces the incorrect text and switches the foreground application to Persian. The reverse direction (Persian → English) is supported as well.

## Why Raaya TypeSwitch?

Switching keyboard layouts manually is easy to forget, especially when working with mixed Persian/English content. The goal of Raaya TypeSwitch is to make the correction unobtrusive:

- 🪶 Lightweight Windows tray application
- 🇮🇷 Persian ↔ English layout detection
- 🔁 Automatic replacement of a mistyped word
- ⌨️ Automatic foreground keyboard-layout switching
- 🔒 Local-first processing; no cloud API is required
- 📵 No runtime CDN, external font, JS, CSS, map, or online service dependency
- 🧩 Detector and Windows integration are separated for future language support

## Requirements

- Windows 10 or Windows 11
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) for building from source
- English (United States) keyboard layout
- Persian keyboard layout

## Getting started

Clone the repository:

```powershell
git clone https://github.com/sam12r18/raaya-TypeSwitch.git
cd raaya-TypeSwitch
```

Build:

```powershell
dotnet build RaayaTypeSwitch.slnx
```

Run:

```powershell
dotnet run --project .\src\RaayaTypeSwitch\RaayaTypeSwitch.csproj
```

After startup, **Raaya TypeSwitch** appears in the Windows system tray.

## Publish a standalone Windows build

To create a self-contained single-file x64 build:

```powershell
dotnet publish .\src\RaayaTypeSwitch\RaayaTypeSwitch.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true
```

Output:

```text
src\RaayaTypeSwitch\bin\Release\net10.0-windows\win-x64\publish\
```

## How it works

Raaya TypeSwitch installs a Windows low-level keyboard hook and keeps a short in-memory buffer of the physical keys used for the current word.

At a word boundary it:

1. Reads the current keyboard layout of the foreground window.
2. Reconstructs the buffered physical keystrokes with both Persian and English layouts.
3. Scores the current and alternate interpretations locally.
4. Corrects only when the alternate interpretation passes a conservative confidence threshold.
5. Deletes the incorrect word, injects the corrected Unicode text, and switches the foreground layout.

The program ignores keyboard events that it injects itself to avoid feedback loops.

More details are available in [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).

## Privacy

The project is designed around a strict local-first model:

- Typed text is not sent to a server.
- No cloud AI or analytics service is required.
- The MVP does not persist the typing buffer to disk.
- The detector operates on a short in-memory word buffer.

**Important:** Password/PIN-field detection is not implemented yet. Until that protection is added and audited, treat this project as an experimental MVP and disable it when entering sensitive information.

See [SECURITY.md](SECURITY.md) for the current security model.

## Current limitations

- Automatic correction is performed at word boundaries, not continuously after each character.
- The built-in Persian and English dictionaries are intentionally small.
- Password/PIN and secure-field detection is not implemented yet.
- Excluded-application rules are not implemented yet.
- Some elevated applications may require Raaya TypeSwitch to run at a matching integrity level.
- Remote Desktop, virtual machines, games, and applications with custom text-input stacks may behave differently.
- Only Persian and English (US) are supported by the detector today.

## Roadmap

Near-term priorities:

- [ ] Secure-field / password detection using Windows UI Automation
- [ ] Excluded applications list
- [ ] Manual “fix last word” hotkey
- [ ] Undo last automatic correction
- [ ] Windows Startup option
- [ ] Larger offline Persian/English dictionaries
- [ ] Local n-gram language scoring
- [ ] Per-application sensitivity
- [ ] Settings UI
- [ ] Automated detector and mapping tests
- [ ] Signed release builds and installer
- [ ] Additional keyboard layouts and languages

## Project structure

```text
RaayaTypeSwitch.slnx
src/
  RaayaTypeSwitch/
    Program.cs
    TrayApplicationContext.cs
    KeyboardHook.cs
    KeyboardLayoutService.cs
    TypingEngine.cs
    LanguageDetector.cs
    TextInjector.cs
    NativeMethods.cs
docs/
  ARCHITECTURE.md
.github/
  workflows/
    build.yml
```

## Contributing

Issues, bug reports, test cases, and pull requests are welcome. Please read [CONTRIBUTING.md](CONTRIBUTING.md) first.

When reporting a layout bug, include:

- Windows version
- Source and target keyboard layouts
- Application where the issue occurred
- Exact intended text
- Exact text produced before correction

Do **not** include passwords, credentials, tokens, or private text in bug reports.

## License

Raaya TypeSwitch is licensed under the [MIT License](LICENSE).

---

Developed as an open-source Raaya utility for reducing Persian/English keyboard-layout typing mistakes.
