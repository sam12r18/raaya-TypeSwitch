# Security Policy

Raaya TypeSwitch observes keyboard events in order to detect wrong-layout typing. That capability requires a strict security model.

## Current status

The repository currently contains an **experimental MVP**. It is not yet recommended for sensitive or enterprise environments.

### Implemented privacy properties

- Detection is local.
- No typed content is sent to a remote API.
- No cloud AI service is required.
- The active word buffer is held only in memory.
- Injected keystrokes are ignored by the hook to prevent recursion.

### Known security gap

**Secure-field / password / PIN detection is not implemented in the MVP.**

Until secure-field detection is implemented and audited, users should disable Raaya TypeSwitch while entering passwords, PINs, recovery codes, API keys, private keys, or other secrets.

## Planned protections

Before a stable release, the project should include:

1. Windows UI Automation checks for password/secure edit controls.
2. Automatic suspension on known credential surfaces.
3. Per-application exclusions.
4. A visible global pause state.
5. No persistence of captured text by default.
6. Security-focused tests around injection, elevated applications, and desktop/session boundaries.
7. Signed binaries and reproducible release guidance.

## Reporting a vulnerability

Do not publish credentials, private typed content, or exploit details containing real secrets in a public issue.

For non-sensitive security hardening suggestions, open a GitHub issue with a minimal reproducible example. For a vulnerability that could expose user input, use GitHub's private vulnerability reporting feature if it is enabled for this repository.
