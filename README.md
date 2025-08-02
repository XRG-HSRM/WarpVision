## Requirements:
- Unity version 2022.3.47f1 (should work on other versions as well)
- Python version 3.10.6 (for data collection in the study, not required otherwise)
- SRanipal (for eye tracking, can be replaced with more recent Open-XR variant -> adjust EyeTrackingRaycast.cs / gazeRay)
- SteamVR
- Tested using the Vive Pro Eye HMD

## Install:
- Simply clone the repository for use in a Unity project

## Study Setup:
- To setup the framework for a study, the python script "data_input" has to be started to allow the collection of data through the researcher
- SRanipal has to run
- Start "StartupScene" to begin study
	- in the controller of the StartupScene, various settings can be adjusted, such as the used scenes, gaze guidance techniques, and maximum duration of a search
- The user (study participant) will need to click the trigger of a VR hand controller to notify the framework of him finding an object
- the VR hand controller trigger will also be used to start the framework
- data will be collected in the Assets/Data folder, this can be adjusted to a persistent datapath


## To use WarpVision in any Unity application, the following files are required/need to be adjusted:
- VRCamera.cs
	- a simple script that allows WarpVision to delegate a custom render function
	- should be a component of the VR camera inside the VR rig
- WarpVision.cs
	- The main script of WarpVision, handles parameters, start, and stop
	- replace controller.currentEyeTrackingScript.gazeRay.origin; and controller.currentEyeTrackingScript.gazeRay.direction; with the origin and direction of the eye tracking
	- replace controller.currentEyeTrackingScript.isValid with validity check of the eye tracker (not required, can be set to true)
	- replace controller.currentEyeTrackingScript.lookingAtSearchObject with a check whether the object that is searched for is currently being looked at
	- StartVisionCatcher() and StopVisionCatcher() start and stop WarpVision
- VisionCatcher
	- Super script of WarpVision
	- transformToCatch is the transform of the object that is being searched for
- WarpVisionMaterial
	- the material that uses the WarpVision shader
- WarpVisionShader
	- the shader to curve the space

## Attribution
The realistic scene is based on material from Amazon Lumberyard (https://developer.nvidia.com/orca/amazon-lumberyard-bistro), used under the [CC BY License](https://creativecommons.org/licenses/by/4.0/).
Modifications were made to the original files.

This repository is provided as-is and is not actively maintained. It is shared for the sake of research transparency and to support open science.
