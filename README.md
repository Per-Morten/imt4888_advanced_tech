# Repository
This repository contains the code written for the IMT4888 Specialization in Game Technology course by Per-Morten.
The repository contains both the Code for the mesh deformation and the code for the visual effects.  
Unfortunately, the code base is in quite a mess so I would recommend reading this before stepping in there so you can more quickly find the things that matter.
The code itself is also in quite a prototype like state, with lots of parts commented out etc as I was testing.

## Mesh Deformation
The mesh deformation is located in the folder simply named Unity.

### Running the Code
I am not entirely sure of the Unity version that is needed to run the Mesh deformation, however, the version I used when I developed it was 2018.1.6, before moving over to Unity 2018.2.8.
In both scenes, you can move around in the game view with WASDQE, provided you hold the right mouse button (similar to how it works in the editor).

#### Ball Deformation
The Ball deformation showed in the video happens in the Scene named CatlikeCodingMeshBasics4MeshDeformation within the CatlikeCoding folder.
The code for this implementation is in the files located in CatlikeCoding/Scripts/Deformation:
* MeshDeformer.cs
* MeshDeformerInput.cs
* TerrainDeformer.cs (Which doesn't do terrain deforming, it does the same deformation as on the ball, just on a grid instead).
* CollisionDeformerInput.cs

You perform deformation on an object by clicking on it in the game view.
The camera holds a script called Mesh Deformer Input, here you can specify the force to be applied to the object, and the offset (How far the force should be applied from the point it hit the ball). 
In the Sphere and Terrain, you can modify the Mesh Deformer variables Spring Force and Damping to control the strength of the springs and how much damping should be applied.
Pressing K and aiming at the ball will create a ring of points to deform. 
I was doing a quick experiment on what would happen if I "pinched" the mesh, did not turn out as expected. 

#### Cloth Simulation
The Cloth simulation as shown in the video happens within the Scene named SpringMeshCloth within the CatlikeCoding folder.
The code for this implementation is in the files located in CatlikeCoding/Scripts/Deformation:
* MassSpringCloth2.cs.

Before you start the application you can specify the number of rows and columns that a piece of cloth should have. 
Do note that the cloth is still just 1 Unity unit, so it is just the number of cells within the system. You need the same number of rows and columns, otherwise, the springs will be attached to the wrong vertices, 
which breaks the cloth. 
The mass of the vertices can be specified in the inspector, but don't at any point allow it to become 0, as I don't do any checks for division by 0 in the code.
Gravity, damping, and number of iterations on constraints can also be changed.
The sphere can be moved around in the editor, or in-game by turning on the Sphere mover script.
Collisions between the cloth and the sphere do not respect the scale of the cloth, so if you scale up the cloth the collisions with the sphere will look a bit awkward.

##### Modifying the code.
If you want the springs drawn, you can comment back in the code in OnDrawGizmos within the MassSpringCloth2.cs file,
however, that will impact performance.

If you want to have the cloth hang from just one vertex, rather than the two, you can simply comment out the part of the code in FixedUpdate below the comment "Fix Two Corners", and comment in the code under the comment "Fix middle point". However, if you do this the cloth is still really elastic, so to get the results I had in my images, you need a very high vertex mass, something like 300 should do.

#### Other Code
My first botched attempt at Writing a mass-spring cloth implementation can be found in the file called MassSpringCloth.cs
Most of the other files are from when I was going through the tutorials on procedural mesh generation,
and tutorials on rendering, as well as rounding a cube in shader rather than on the CPU.

## Visual Effects (Reflection Code)
The folder named shader within projects_for_reflection contains some of the visual effects I created when I was looking into shader graph
To run this code you need at least Unity 2018.1 as that was when Shader Graph was introduced.
The demos shown in the video can be seen in the Scene new_scene under Scenes. 
To change between the different effects you can simply change the shader of the Sphere. 
The shaders shield_effect, dissolve, hologram_shader, and teleport_shader should work with all models,
but the ones named rock_shader, and hard_hat was written when I tried to employ bump mapping for the safety_hat model, and the rock model in the scene (that are disabled).
