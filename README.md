# Card Interaction Experiment (Godot/C#)

An experiment with building an interface where you are looking at a table with cards, and are able to move them around.

Inspired by [Cultist Simulator](https://store.steampowered.com/app/718670/Cultist_Simulator/) primarily, a type of game I want to try building.

![](./Animation.gif)

Biggest challenges:

- all cards are 0 thick, and co-exist on the table plane (which is also 0 thick)
- this causes massive depth fighting, so there is a need to override render order
- cards have transparent edges, so fixes for render order also need to make sure transparency continues to work
- cards also have a glow effect when hovered over and in some interactions, which is a shader trick (and involves transparency)
- dragging and dropping (and hovering) should result in a card being on top, and should affect only the card clicked even if it overlaps multiple cards

A few approaches were tried for the key issue, the z-fighting:

- minute y-height changes (the table/cards face up on the y axis) were not enough for the depth buffer
  - also the perspective camera, which views at an angle, would result in ordering
    getting messed up especially near the edge of a screen (where a dragged card might be 'higher'
    than another card, but is further away from the perspective of the camera)
- by rendering cards as 2D sprites under a subviewport, then using a viewport texture I could get things to work, but this was complex and limiting
  - the math to convert 3d raycasts from the mouse through the table object and the viewport so the 2d objects could receive them and work accordingly was involved, but workable (this sample project does the same thing, though I painfully finished mine before I found it and saw it did the exact same thing :D <https://github.com/godotengine/godot-demo-projects/tree/master/viewport/gui_in_3d>)
  - it was limiting because while I could put 3d objects on top of the board, if I wanted a card when dragged to be above those 3D objects it wasn't going to work
- by manually overriding DEPTH in shaders (with depth_draw_always), but this broke transparency
  - it solved my depth fighting issues, but by setting a depth value on transparent or would-be transparent pixels, they completely occluded pixels behind them. no pixels, no blending so the tranparent areas always just mixed with the clear colour of the camera (black)
  - alpha scissoring, by discarding such fragments when they were below a threshold rather than setting DEPTH for them, solved the camera blend issue but transparency was still effectively removed
- using shader render priority
  - this approach worked and solved all problems, except one: the render priority value on shaders is a 8-bit number (well a signed 7-bit number, so -128 to 127)
  - this would mean I could only have 256 objects in the scene, if they all had their own order
  - solved with a somewhat complex function (not too bad) that would reorder overlapping objects, so only a few discrete render order values were needed at any given time (objects can share an order if they don't overlap, obviously)
  - also, to ensure depth testing didn't mess with my precise render ordering, its turned off in the shader via depth_test_disabled.

All worked rather swimmingly at the end 🙂 Note, adding 3D objects in here will work rather weird without additional tuning: the cards will appear above them even though under (they have shadow reception disabled so at least they won't appear black). To fix, the 3D shapes probably also need depth testing disabled though this will prevent occlusion and a few other things from working right. A problem for the next project.

**Resource Acknowledgement**: the felt green background texture is from <https://opengameart.org/content/felt-backgrounds> by <https://opengameart.org/users/jbp4444>
