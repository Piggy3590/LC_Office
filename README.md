# LC_Office

![Preview2](https://i.imgur.com/qRbVe3l.png)

![Preview1](https://i.imgur.com/F2CT8C8.png)

Adds new office-themed interior and new enemy!

This mod adds **Shrimp** from Zeekerss' previous game, *The Upturned*.

map generation is still in a very early stage and the maps have no decoration.

Since the mod is still in development, it may be unstable.

# Credits

Models and rigs are from [The Upturned](https://store.steampowered.com/app/1717770/The_Upturned/) by Zeekerrs, which I will remove if it becomes an issue.

[The Upturned](https://store.steampowered.com/app/1717770/The_Upturned/) is also an awesome game :D Give it a try


Thanks to:
Woecust for modeling the interior (and check out his mod, [Immersive Visor](https://thunderstore.io/c/lethal-company/p/Woecust/Immersive_Visor)!).

IAmBatby, the developer of LLL, for all his help in moving to [LethalLevelLoader](https://thunderstore.io/c/lethal-company/p/IAmBatby/LethalLevelLoader)!

Xilo (xilophor), the developer of [LethalNetworkAPI](https://thunderstore.io/c/lethal-company/p/xilophor/LethalNetworkAPI/), who provided a quick hotfix when a problem occurred.

TheHomelessHobo, for his great help with writing the bestiary.


music:

+ REGRETEVATOR OST - bossa lullaby
+ nico's nextbots ost - shop
+ nico's nextbots ost - safe room
+ nico's nextbots ost - shop

Every music was composed by nicopatty, special thanks to nicopatty for the great response to the mod!!!

# ChangeLog
**0.0.3**  

	- Remove test item spawn.

**0.0.4** 

	- moved to LethalLevelLoader.
 
 	- Changed the layout of the server racks.
 
	- Fixed an issue where some items would not set the elevator as their parent object.
 
	- Fixed some errors that occurred during interior generation.
 
	- Lots of optimization work.
 
	- Sadly no monster spawns yet :/

**0.0.5** 

	- edited README.md

**0.0.6** 

	- Changed the layout of the some room.

**0.0.7** 

	- Fixed issue with guaranteed spawns on any moons.

	- Changed the layout of the some room.

	- Every song filters have been edited to make them less painful to the ears.

	- Removed the cootie theme.

	- Fixed the pitch of the shop theme the same as the original.

**0.0.8** 

	- Fixed an issue with guaranteed spawns on every moons. now appears randomly on paid moons.
	
	- Added configuration for interior spawns.

	- Temporarily completed the unfinished start room.

	- Created a staircase on the left side of the start room (ladder included, so two handed items cannot be moved).

	- Fixed extreme frame drops in game before landing or when on a moon with no office interiors spawned.

	- Revamped trap rooms (still unfinished, you won't see any changes in actual gameplay).

	- Added [OdinSerializer](https://thunderstore.io/c/lethal-company/p/Lordfirespeed/OdinSerializer/) as a dependency in preparation for LethalNetworkAPI v2.0.0.

**0.0.9** 

	- Fixed some issues.

**0.1.0**

	- Enemies can now spawn!

	- Added a new enemy "Shrimp"!

	- On second thought, OdinSerializer is a dependency of LethalNetworkAPI, so it should be installed automatically even if it's not there. Removed OdinSerializer from the dependency.

	- Fixed an issue where navmesh agents could not navigate through staircase rooms (even though no enemies spawned)

	- Compatible with LethalNetworkAPI 2.1.0.
	
**0.1.1**

	- Added pictures to README.md.
	- Fixed extreme frame drops occurring when an interior other than an office was created.
	- Reduced the time it takes for an enraged Shrimp to reach its maximum speed(acceleration remains the same).
	- Added configuration related to the spawning of Shrimp.
**0.1.2**

	- Added “Satisfaction” value to Shrimp. Shrimp will now stop following the player for a while if they have enough food. (not tested)
	- Fixed softlock occurring when shrimp spawn on all clients except the host.
**0.1.3**

	- Added two large rooms.
	- Added a prop to several rooms.
	- Remodeled the starting room.
	- Improved dungeon generation.
	- Added Lung apparatus socket machine spawn.
	- improved the elevator system, but haven't tested it yet.
	- Fixed a bug where Shrimp's "Satisfaction" system did not work properly.
**0.1.4**

	s m o l Update
	- Minor performance improvements.
