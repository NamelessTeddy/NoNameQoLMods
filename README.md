# NoName's Stardew QoL Mods

A SMAPI mod for Stardew Valley that adds a configurable keybind to cancel tool/weapon animations, with optional auto-use looping.

Originally by [Swaglix05](https://github.com/Swaglix05/AnimationCancelKey). This fork adds auto-use loop behavior and other improvements.

## Features

- **Tap** the cancel key during a tool use → animation is cancelled after a short delay (one use + cancel)
- **Hold** the cancel key → continuously uses the tool and cancels the animation in a loop
- Fishing rods are excluded and work normally
- Configurable via [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) (optional)

## Configuration

| Option | Default | Description |
|---|---|---|
| **Cancel Key** | `Space` | The key to hold/press to trigger animation cancelling |
| **Cancel Delay** | `220 ms` | How long to wait after a tool use starts before cancelling the animation. Lower = faster but may cancel before the effect registers |
| **Suppression** | `OnCancel` | When to suppress the cancel key's other in-game functions. `OnCancel` = only when cancelling; `Always` = every time it's pressed |

## Changes from original

- **Auto-use loop**: holding the key now continuously uses the tool and cancels animations, rather than only cancelling a single in-progress animation
- **Tap support**: a brief press now also cancels the animation (timer runs to completion even if key is released)
- **Configurable delay**: cancel timing is exposed as a GMCM option (50–1000 ms, default 220 ms)
- **Fishing rod exclusion**: fishing rods are now filtered out so they work normally

## Building

1. Install the [.NET SDK](https://dotnet.microsoft.com/download) (8.0 or later)
2. Clone the repo and navigate to the project folder:
   ```bash
   git clone https://github.com/NamelessTeddy/AnimationCancelKey.git
   cd AnimationCancelKey/NoNameQoLMods
   ```
3. Build and deploy:
   ```bash
   dotnet build
   ```
   The mod is automatically copied to your Stardew Valley `Mods/` folder on a successful build.

## Requirements

- [SMAPI](https://smapi.io/) 4.0+
- Stardew Valley 1.6+
- [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) (optional, for in-game config)
