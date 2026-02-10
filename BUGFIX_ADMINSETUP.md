# Bug Fix: /adminsetup "Unknown Command" Error

## Problem
When users typed `/adminsetup` in chat, they received an "unknown command" error.

## Root Cause
The `AdminSetupCommand` method had two conflicting attributes:
```csharp
[ChatCommand("adminsetup")]
[ConsoleCommand("adminsetup")]
private void AdminSetupCommand(BasePlayer player, string command, string[] args)
```

In Oxide/Rust plugins, you **cannot use both `[ChatCommand]` and `[ConsoleCommand]` on the same method** because they require different method signatures:

- **ChatCommand**: `void Method(BasePlayer player, string command, string[] args)`
- **ConsoleCommand**: `void Method(ConsoleSystem.Arg arg)`

The dual attributes caused a conflict that prevented the command from registering properly.

## Solution
Removed the `[ConsoleCommand("adminsetup")]` attribute, keeping only the chat command:

```csharp
[ChatCommand("adminsetup")]
private void AdminSetupCommand(BasePlayer player, string command, string[] args)
```

## Result
✅ The `/adminsetup` command now works correctly as a chat command
✅ Users can type `/adminsetup` to open the admin UI panel
✅ No more "unknown command" errors

## Usage
Simply type in chat:
```
/adminsetup
```

The graphical admin setup panel will open.

## Files Changed
- `PaintballArena.cs` - Removed conflicting attribute
- `ADMIN_UI_GUIDE.md` - Updated to reflect chat-only command
- `UI_IMPLEMENTATION_SUMMARY.md` - Updated code examples

## Note
The console commands for UI buttons (`adminsetup.setgate`, `adminsetup.selectarena`, etc.) are unaffected and continue to work properly because they use the correct `ConsoleSystem.Arg` signature.
