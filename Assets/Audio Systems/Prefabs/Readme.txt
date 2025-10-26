----UI_VolumeSliders----

Contains all the UI GameObjects for the volume sliders.
These are set up to work with the "MenuVolumeControls" and "VolumeSlider" scripts, also included in the package.

----AudioManager----

Empty GameObject with the "AudioManager" and "MenuVolumeControls" scripts attatched.
Also has an empty child GameObject called "FMODEvents" that has the "FMODEvents" script attatched.

----AreaChangeTrigger----

Empty GameObject with a box collider, and the scripts "MusicChangeTrigger" and "AmbienceChangeTrigger" attatched.
We can use this whenever we need to trigger a change in audio, upon the player entering an area.
(eg. you enter the forest = forest ambeience + music plays, you enter the desert = desert ambience + music plays)