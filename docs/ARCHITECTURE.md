# Architecture

Raaya TypeSwitch separates keyboard capture, layout reconstruction, detection, and text replacement so each concern can evolve independently.

## Input flow

```text
Foreground Windows application
          |
          v
Low-level keyboard hook (WH_KEYBOARD_LL)
          |
          v
Short physical-key buffer
          |
          +-------------------------------+
          |                               |
          v                               v
Current active layout              Alternate layout
          |                               |
          v                               v
ToUnicodeEx reconstruction         ToUnicodeEx reconstruction
          \                               /
           \                             /
            v                           v
                LanguageDetector
                      |
              confidence decision
                      |
         +------------+-------------+
         |                          |
         v                          v
      no-op                   TextInjector
                                    |
                                    v
                          Switch foreground layout
```

## Components

### KeyboardHook

Installs a `WH_KEYBOARD_LL` hook and forwards non-injected key-down events to `TypingEngine`. Injected events are ignored so replacement text does not feed back into the detector.

### TypingEngine

Maintains only the physical keys for the current word. It clears the buffer on navigation/modifier scenarios and evaluates the word when Space, Enter, or Tab is pressed.

Automatic replacement is performed only when the alternate interpretation has a sufficiently stronger language score.

### KeyboardLayoutService

Reads the foreground thread's keyboard layout and uses `ToUnicodeEx` to reconstruct the same physical key sequence under Persian and English (US) layouts.

### LanguageDetector

The MVP uses script checks, a small offline common-word dictionary, common character chunks, a basic English no-vowel penalty, and a confidence margin.

### TextInjector

Uses `SendInput` to remove the incorrect word, inject corrected Unicode text, and re-inject the delimiter.

### Layout switching

`WM_INPUTLANGCHANGEREQUEST` is posted to the foreground window using the Persian or English layout handle.

## Security boundary

A keyboard hook sees input before most applications process it. Secure-field detection is therefore a blocker for a stable release.

The stable design must suspend capture for password controls, credential surfaces, user-excluded applications, and unsupported secure desktops where appropriate.

See `SECURITY.md`.

## Future extension points

- Pluggable language scorers
- Multiple language pairs
- Per-application policy
- Manual correction command
- Undo
- UI Automation-based secure-field guard
- Testable input/layout abstractions
