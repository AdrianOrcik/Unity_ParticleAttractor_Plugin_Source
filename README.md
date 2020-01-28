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
* Ability setup effect without coding
* Similar to Particle System
* Two spawn modes (OneFrame,SeveralFrames)
* Ability draw particle path by hand
* Handy tool inspector
* Easy API to use

:card_file_box: API
---------
#### Scenario API
Following functions are using for particle scenario.

* Transform_DoScale(Vector3 toScale, float duration)
  * Scale in duration time
* Transform_DoScale_UpDown(Vector3 toScaleUp,Vector3 toScaleDown, float duration)
  * ScaleUp and scaleDown in duration time (using for popup effect)
* Transform_DoMove(float duration)
  * Move obj from Source to Destination property in duration time
* Transform_DoMove_SetTarget(Transform toTransform, float duration)
  * Move obj from the source property to target transform in duration time
* Transform_DoRotate(Vector3 toRotate, float duration)
  * Sequence rotate obj into a rotate direction in duration time 
* SpriteRenderer_DoFade(float toFade, float duration)
  * Sequence make a fade effect on sprite from actual value to fade value in duration time
  * fade value range 0 to 255
* Sequence_DoDelayInRange(Vector2 rangeFloatParam)
  * System sequence which does delay before the next sequence from scenario will be invoked
* Tweener Transform_DODrawMove(float duration)
  * Sequence move obj across hand defined path

#### Particle Attractor API
Particle Attractor API is powered by Builder Patter for universal and easy usability.

* SetSourceTransform( Transform source )
* SetDestinationTransform( Transform destination )
* SetSpawnImage( Sprite image )
* SetSpawnAmount( int amount )
* InvokeSpawn()
* InvokeSafeSpawn()
  * Attractor check own state, if should be invoked
* InvokeSafeSpawn(Transform sourcePosition)
* Attractor check own state, if should be invoked
  * sourceTransform used as a default source of particle spawn
* SetAttractorSetupState( bool value)
  * Setup if attractor is configured and ready for Invoke → means this config will be used by next spawn
  * Used in special cases when Invoke is called from another place then config
* SetAsActive()
  * Means currently PA has an active process
* SetAsDeactivated()
  * Means currently PA has not active process & is ready for use

:page_facing_up: Examples
--------- 
#### Simple Effect
![SimpleAttractorEffect](https://user-images.githubusercontent.com/14979589/73284387-29adaa80-41fd-11ea-8229-16e46664aa7a.gif)

#### Drawed Effect 
![DrawAttractorEffect](https://user-images.githubusercontent.com/14979589/73284391-2aded780-41fd-11ea-8573-99ed373e4bda.gif)

:package: Installing
---------
//TODO: add description
