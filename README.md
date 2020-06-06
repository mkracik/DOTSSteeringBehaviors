# DOTS Steering Behaviors

This repository contains a partial implementation of [Steering Behaviors](https://www.red3d.com/cwr/steer/gdc99/) using [Unity Data Oriented Tech Stack](https://unity.com/dots). It was initially based on C++ library [OpenSteer](http://opensteer.sourceforge.net/).

It has been used to control ship movement in game [Chronostation](https://store.steampowered.com/app/1217900/Chronostation/).

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes.

### Prerequisites

* Unity version 2019.3.15f1
* Unity package Entities 0.11.0 preview.7
* Unity package Hybrid Renderer 0.5.1 preview.18

(Unity will automatically download the packages from file Packages/manifest.json)

### Installing

* Clone this repository into local directory *DOTSSteeringBehaviors*.
* Add the directory *DOTSSteeringBehaviors* as a new project in Unity Hub.
* Open the project in Unity.
* Open and start the example scene *Assets/Scenes/SampleScene.unity*

## How to use
To use in your own project, copy the directory *Assets/Scripts/SteeringBehaviors/* into your project's Assets directory. Assembly SteeringBehaviors is automatically referenced.

* Add all these components to your entity prefab:
*SimpleVehicle, SBPosition2D, SBRotation2D, SBVelocity2D*.
* Add at least one of these components to your entity prefab: *Arrival, Leash, Wander, SBAvoidanceSource*.
* Add component *SBAvoidanceTarget* to entities which should be avoided.

You can use Unity's GameObject conversion system like in the included sample prefab *Assets/\_Complete-Game/Prefabs/Done_Player.prefab*; or you can add the components to your entity or archetype in code.

* Spawn entities, for example like in method SpawnRandomShips() in *Assets/Scripts/SampleSceneController.cs*

* The simulation is using its own components *SBPosition2D* and *SBRotation2D*. To actually draw, move and rotate Unity entities with Hybrid Renderer, they must be copied to Unity's components *Translation* and *Rotation*, as in the example systems *Assets/Scripts/UnityTranslationUpdateSystem.cs* and *Assets/Scripts/UnityRotationUpdateSystem.cs*. 

## Notes
*Arrival* behavior is modified so that ships can smoothly slow down at a predefined distance from their target.

*Unaligned Collision Avoidance* behavior needs fine tuning with regard to ship speed and target visibility range. It is trying to locally avoid one nearest ship and sometimes collides with another ship. Its algorithm's complexity is *O(N<sup>2</sup>)*: every *SBAvoidanceSource* avoids every *SBAvoidanceTarget* in its visibility range. It is optimized from OpenSteer's straightforward implementation with Unity DOTS: inner loop data is precomputed once and stored in a cache-friendly array; and Unity's Burst compiler is used. As a result, in later stages of the game Chronostation there can be hundreds of ships and targets while maintaining 60 FPS on low-end hardware.

Physics is implemented very simply (only maximum speed works; mass and maximum force do nothing) because it was not adding to realism but was introducing variables which were difficult to tune. It would be best to use [Unity DOTS Physics package](https://docs.unity3d.com/Packages/com.unity.physics@0.0/manual/index.html) instead of trying to implement own physics.

This implementation shares [issues](https://andrewfray.wordpress.com/2013/02/20/steering-behaviours-are-doing-it-wrong/) with OpenSteer library and Steering Behaviors in general.
Steering Behaviors work nicely as simple isolated examples, but creating and fine-tuning realistic in-game behavior is difficult.


## Authors

* [Michal Kracik](https://github.com/mkracik)

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments
* [Steering Behaviors For Autonomous Characters](https://www.red3d.com/cwr/steer/gdc99/).
* [OpenSteer C++ library](http://opensteer.sourceforge.net/).
* [Unity Space Shooter Tutorial](https://github.com/lukearmstrong/unity-tutorial-space-shooter) for the example spaceship model.
