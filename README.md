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
	- Minor performance improvements for items.
	- Fixed some bugs in the shrimp.

**0.1.5**

	- Fixed a bug where elevator safety doors would not open when starting the game.
	- Fixed a bug that caused RoundMapSystem to consistently error out on loading.
	- The emergency exit is a bug we can't fix at the moment due to an error in LLL (or a DungeonFlow error in Office), and I'm talking to the LLL developers - sorry!

**0.1.6**

	- Fixed some bugs.

**0.1.7**

	- Added a new facility style stair room to replace the old stair room.
	- Added a small LED next to the elevator panel to make it easier to see if the elevator is up.
	- Remove some rooms (two door rooms and rooms with stairs) because the original file was broken... sad
	- Removed test room.

	- Made some improvements to dungeon creation.
	- Reduced the overall size of assets.
	- (Probably) fixed the sliding glass door. Once again.
	- Fixed a bug where spray paint/blood was not visible.
	- Changed the name from ShrimpSpawnableMoons to ShrimpSpawnWeight, so you need to reconfigure it. 
	- Changed the text on the elevator panel.
	- Fixed an issue where shrimp would not spawn properly in office interiors.

**0.1.8**

	- Shrimp now spawns on a modded moon.
	- Added configuration for Shrimp to set spawn weight on modded moon.

**0.1.9**

	- Fixed a bug where staircase rooms had incorrect colliders.

**0.2.0**

	- This update uses a lot of network variables and may cause some frame drops. Please let me know if you are experiencing severe frame drops.

	- Synced Shrimp's hunger value.
	- Separated the stairs and hallway in the Staircase room.

	- Fixed a bug where:
	- Shrimp could not open doors.
	- Shrimp would occasionally look in strange directions.
	- Shrimp's position and orientation were out of sync.
	- Shrimp would not react when hit.

	- Fixed a bug where you could get stuck in the walls of the staircase room.
	- Fixed a bug where some rooms would spawn incorrect doors/no doors.

**0.2.1**

	- Fixed bug where rooms would spawn overlapping.
	- Fixed a bug where players who used the elevator even once would not respawn on takeoff.
	- Fixed bug causing doors to be misplaced.
	- Added exception handling for shrimp not syncing bug. Please let me know if you encounter this bug.

**0.2.2**

	- Fixed bug where turrets would penetrate some walls.

**0.2.3**

	- Added auxiliary power units to elevators(Compatible with FacilityMeltdown mod).
	- If a apparatus within the interior was unplugged, the elevator was changed to be inoperable without the use of auxiliary power
	
	- Shrimp desync issue is probably.. fixed.. again.. idk
	- Fixed a bug where doors in some rooms were not positioned correctly.

**0.2.4**

	- Added a ladder in case the elevator is unavailable for some reason.

	- Fixed a bug where the doors would not open when the elevator automatically descended after taking Apparatus.
	- Fixed a bug where rooms would spawn overlapping due to forgetting to set the bounds override

**0.2.5**

	- Added little animation when placing Apparatus in an elevator.

	- Fixed several bugs that occurred when lobby restarted.
	- Fixed a bug where the doors would not open when the elevator automatically descended after taking Apparatus. again.

**0.2.6**

	- Made some minor improvements to the elevator doors.