Thank you for the purchase. Hope the assets work well.
If they don't, please contact me: slsovest@gmail.com.


______________________
Assembling the robots:

Legs, shoulders and cockpits contain sockets for mounting other parts, their names start with "Mount_".
Just drop the part in the corresponding container, and It'll snap into place.

- Start with legs. 
- The first container is in Legs->HIPS->Pelvis->Top->Mount_top. 
- Put the shoulders or the cockpit into "Mount_top".
- Find other containers in shoulders and cockpit.

After the assembly, robots consist of many separate parts and, even with batching, produce high number of draw calls.
You may want to combine non-animated parts into a single mesh for the sake of optimization.

All weapons contain locators at their barrel ends (named "Barrel_end").

If you need a cap for the hole at the top of the legs, there's a prefab named "Legs_top_cap". Just drop it into "Mount_top" container.


___________
Animations:

If you want to tweak the animations or create the new ones, the rigged legs files could be fould in Animations -> Heavy_Legs_Rig.rar.
Jump_jetpack animation consists of 3 stages:
- jump start (1-15)
- jump idle (15-56)
- jump land (56-67)
Idle animation has 2 variations
- idle (1-300)
- idle_simple (170-260)
(The animations should be already organized in the project. 
But in case you are using it outside unity, or something breaks in other versions, you could use these numbers.)


_____________________________
The source texture .PSD file:

Can be downloaded here:
https://drive.google.com/file/d/1XKHlcus-bYXPclUi1wkRhgbsvWBuLoY0/view?usp=sharing

For a quick repaint, adjust the layers in the "COLOR" folder. You can drop your decals and textures (camouflage, for example) in the folder as well. Just be careful with texture seams.
You may want to turn off the "FX_Rust" and "FX_Chipped_paint" layers for more cartoony look.
The baked occlusion and contours may be found in the "BKP" folder.


________
Updates:

Version 1.1:
Added new animations - death, jetpack jump and turn on place (30degrees per animation).
Added couple of new texture variations.

Version 1.2:
Fixed Root Motion by adding an additional parent joint to all the legs.
Tweaked walk and run animations slightly.
Changed white and black textures to match spider pack better.
Added new Shoulders part, tweaked lvl1 shoulders.
Added 3 HalfShoulder parts (can be useful with the spiders pack).

Version 2.0:
Normal map and PBR textures added!
New animations:
- turning while walking/running
- walking backwards
- faster turning on place

Version 2.1:
Refined the normal map

Version 2.2:
- Equalized the texture brightness across all the Mech Constructor packages
- Added Metalness map
- Added HDRP Mask map
