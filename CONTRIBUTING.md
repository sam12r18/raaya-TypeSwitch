# Contributing to Raaya TypeSwitch

Thanks for helping improve Raaya TypeSwitch.

## Development requirements

- Windows 10/11
- .NET 10 SDK
- English (US) keyboard layout
- Persian keyboard layout

## Build

```powershell
dotnet restore
dotnet build RaayaTypeSwitch.slnx
```

## Run

```powershell
dotnet run --project .\src\RaayaTypeSwitch\RaayaTypeSwitch.csproj
```

## Pull requests

Keep changes focused and describe:

1. What problem is being solved.
2. How the change behaves.
3. Which applications were tested.
4. Which Persian/English samples were tested.
5. Any security/privacy implications.

## Keyboard-layout bug reports

Useful reports include:

- Windows version
- Application name/version
- Current layout
- Intended language
- Physical-key sequence or resulting text
- Expected corrected text

Never include real passwords, authentication codes, tokens, or confidential text.

## Design principles

- Local-first by default.
- Do not persist typed text unless a future feature explicitly requires it and the user opts in.
- Prefer conservative correction over false positives.
- Keep Windows-specific input code isolated from language-detection code.
- Avoid runtime dependencies on CDNs or remote assets/services.
