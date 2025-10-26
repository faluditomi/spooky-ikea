----AUDIOMANAGER----

Singleton that contains all Audio/FMOD related methods.
Can be accessed from any other script ("AudioManager.instance")

Instances created using the "CreateEventInstance" method, are added to the list "eventInstances".
Emitters initialized using the "InitializeEventEmitter" method, are added to the list "eventEmitters".
The "CleanUp" method stops playback of all emitters and instances within the lists, 
and releases the instances from memory.(Can be called when changing scenes, to ensure that everything that shouldn't be playing in the new scene,
is stopped)

From here we can also specify what music and ambience should play in which scene, using a switch method.
This way, the correct music and ambience events will be initialized, whenever a scene is loaded.

----FMODEvents----

Singleton that contains references to all FMOD Events.
Can be accessed from any other script ("FMODEvents.instance")

This script allows us to access the FMOD Events from anywhere, and we need to be able to do that,
as some of the methods in the AudioManager, must be fed with an EventReference.


----MenuVolumeControls----

The MenuVolumeControls script contains the float variables for the different volumes. These variables are defined as float ranges, where 1 = full volume, and 0 = completely inaudible.
The volume float variables are also initiated in the Awake method.
We also define our Bus variables for the busses in FMOD, and assign these in the awake function.
Loads saved volume settings from the PlayerPrefs class data.
Assigns our Bus variables to the correct bus in FMOD.
Updates the volume of the FMOD busses every frame.

----VolumeSlider----

This is where the value of our sliders, are fed into our volume float variables (those defined in the MenuVolumeControls script), 
that control the busses in FMOD.
Every time a slider's value is changed by the player, we check which enumerator that slider is assigned, 
and set the associated volume float variable, to the value of that slider.
Volume settings are also saved (OnSliderValueChagned) and initialized (Awake) in this script.

----MusicArea----

A public enumerator.
We can use this to switch the music, when entering an area.
The values of the enumerator, can be fed into FMOD calling the ".setParameterByName()" method on an EventInstance,
to set FMOD game paramaters that can be set in FMOD to cause certain changes in the music.

----MusicChangeTrigger----

Can be put on GameObjects with colliders, to cause a change in the music using the "OnTriggerEnter()" method.
When an object with the specified tag enters the trigger collider, the assigned enumerator is fed
to a method in the AudioManager, that sets an FMOD game parameter.

----AmbienceChangeTrigger----

Can be put on GameObjects with colliders, to cause a change in the ambience audio using the "OnTriggerEnter()" method.
Checks to see if the player has collided with the zone, and then calls the SetAmbienceParameter method from the AudioManager class,
setting a speciific parameter by a specific value. Thes are both entered in the inspector.

----SurfaceMaterial----

A public enumerator.
We can use the values of this enumerator, to make changes to audio, based on the surface material of objects in the game world.
For instance; the sound of the player's footsteps change to match the surface they are walking on.