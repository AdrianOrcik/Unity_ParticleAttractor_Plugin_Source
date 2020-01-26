:rocket: Particle Attractor
---------
(Unity open source plugin) - Package contains open source code
Boost your UI effect to another level.

![image](https://user-images.githubusercontent.com/14979589/73138438-2a163c00-406b-11ea-9c1c-a280e6b5b35c.png)

:bulb: Idea
---------
Develop particle attractor tool with a focus to the juicy specific situation in-game. For example, rewarding players or buying items in the shop, etc. The main reason for the tool is to give players better feelings from specific situations and from a game. The tool should be able to give non-technical department ability for easy use in workflow by a handy editor.

:pushpin: Architecture
---------
![image](https://user-images.githubusercontent.com/14979589/73138330-13231a00-406a-11ea-80fe-034968d3fd27.png)

* DOTweenCore
  * Using DOTween as the main handler for particle attraction 
* ParticleAttractor(Custom Inspector)
  * Editor site of a tool, handle an ability to creating attraction scenarios
* ParticleAttractor
  * Particle manager which handle performance and use pre-defined particle scenarios from the stage before
* ParticleAttractorHelper
  * The static class which contain supported methods like data getters or data structures
* API
  * Prepared accessibility for easy implementation in the game
  
:pencil: List of features 
---------
* Familiar with Particle System
* Ability to create a scenario for particles
* Two spawn modes
* Ability draw particle path
* Handy tool inspector
* Easy API to use

:card_file_box: API
---------
//TODO: scenario api
//TODO: attractor api

:page_facing_up: Examples
---------
//TODO: add examples (using of api)

:package: Installing
---------
//TODO: add description
