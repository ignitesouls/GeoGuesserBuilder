# GeoGuesserBuilder

**GeoGuesserBuilder** is a desktop application that allows a game show host to generate zipped Elden Ring mod files for use during in-game GeoGuesser-style competitions. These mods are designed to support live shows and player challenges.

## What is in-game GeoGuesser?

Inspired by [Zoodle](https://twitch.tv/zoodle)’s **Soulsguesser**, this format is a live-streamed event where four players compete to identify in-game locations from partial screenshots. Here's how it works in **Elden Ring**:

1. At the start of a round, all players warp to the **Roundtable Hold**.
2. The host shares a cropped or partially obscured screenshot of a location in the game.
3. Players must search the game world to find the matching location.
4. The first player to reach the location scores the most points; subsequent players earn fewer points.
5. If no one finds the location within a set time (e.g., 5 minutes), the host reveals more of the image.
6. This continues until the full image is shown or all players have found the location.
7. The player with the most points at the end of all rounds wins.

## Features for the Game Show Host

- **Live Elden Ring Integration**: GeoGuesserBuilder attaches to a running instance of Elden Ring, using internals from [EldenRingTool](https://github.com/kh0nsu/EldenRingTool).
- **Capture Game Locations**: Capture your character’s current location in-game. Up to 20 locations can be saved per session.
- **Generate GeoGuessing Mod**: Create a mod that places hidden white soapstone messages at the captured locations.
  - Messages are only visible when a player is standing close enough to interact with them.
- **Launch for Testing**: Test your modded locations within Elden Ring before distributing.
- **Export to Zip**: Package the mod files into a zip archive for easy distribution to players.

## Features for Show Participants

- **Fully Unlocked World**: All Sites of Grace and map regions are unlocked from the start.
- **Flexible Starting Build**: All players begin as level 1 Wretches, but are given enough runes to level up to 200.
- **Convenient Starting Gear**: Upon warping to Roundtable Hold, players receive:
  - Maxed-out estus flasks (both red and blue)
  - Spectral Steed Whistle
  - All cracked pots, ritual pots, perfume bottles, and memory stones
- **Expanded Shop Inventory**: The Twin Maiden Husks sell **all** weapons, armor sets, talismans, physick tears, ashes of war, and spells. All of it for free.
- **Free Upgrades**: Weapons can be upgraded freely, without needing smithing stones or runes.

## Acknowledgements

Special thanks to the authors and maintainers of [EldenRingTool](https://github.com/kh0nsu/EldenRingTool), whose work made the core functionality of this application possible.

## License

This project is licensed under the [GNU General Public License v3.0](https://www.gnu.org/licenses/gpl-3.0.en.html).

You are free to use, modify, and distribute this software under the terms of the GPL-3.0 license.

Please note that this license applies **only** to the contents of this repository. Dependencies such as [EldenRingTool](https://github.com/kh0nsu/EldenRingTool) are not covered by this license and may be subject to their own licensing restrictions.
