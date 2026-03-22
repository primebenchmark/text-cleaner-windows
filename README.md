# TextCleaner

A Windows desktop app that automatically cleans pasted text by applying configurable character substitution rules. Built with WinUI 3 and .NET 8.

## Features

- **Auto clipboard monitoring** — watches the clipboard every second and auto-processes new text
- **Real-time processing** — output updates as you type in the input box
- **Auto-copy** — cleaned output is automatically copied back to the clipboard
- **Configurable rules** — add, edit, remove, or reset character substitution rules via the Settings dialog
- **Dark/Light theme** — toggle via the switch in the toolbar
- **Persistent settings** — rules, theme, window size, and font size are saved across sessions

## Default Rules

| Find | Replace With |
|------|-------------|
| `&nbsp;` | *(remove)* |
| `\u00A0` (non-breaking space) | *(remove)* |
| `$rightarrow$` | `➜` |
| `` ` `` | *(remove)* |
| `\` | *(remove)* |
| `*` | *(remove)* |
| `-` | *(remove)* |
| `#` | `❱` |
| `\|` | `❱` |

## Requirements

- Windows 10 version 1809 (build 17763) or later
- [Windows App Runtime](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/downloads) (bundled in the installer)

## Build

```bash
dotnet build -c Release -r win-x64
```

To publish a self-contained executable:

```bash
dotnet publish -c Release -r win-x64 --self-contained true
```

## Installer

The installer is built with [Inno Setup](https://jrsoftware.org/isinfo.php) using `installer.iss`. It bundles the published output and the Windows App Runtime redistributable.

## Settings Storage

Settings are saved to:
```
%LocalAppData%\TextCleaner\settings.json
```
