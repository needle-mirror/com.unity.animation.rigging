
# Rigging Workflow

This document describes all of the steps and components required to create a custom Animation Rig. This [**video**](https://youtu.be/DzW_jQGO1dc) also demonstrates the process. 

## Core Components

Animation Rigs in Unity make use of four key components:

* **Animator:** When added to some root GameObject, this built-in component designates that a hierarchy is animatable. _Animation Rigging works on top of Unity's animation system, and so requires an Animator._
* **Rig Builder:** When added to the root GameObject, this component allows the animated hierarchy to be modified by one or more Rigs, which it assembles into layers.
* **Rig:** When added to GameObjects in an animatable hierarchy, Rigs act as containers for source objects and Constraints that modify the animated hierarchy's pose. You can adjust the weight of a Rig to control its contribution to the final pose.
* **Constraints:** When added to Rig GameObjects or their children, these components specify operations a Rig will perform.

The following illustration depicts a schematic overview of the interdependencies among all of the required types of components.

![Rig Setup Overview](images/rig_setup/rig_setup_overview.png)

Once you have added a Rig Builder component to an animatable hierarchy's root GameObject, you can create a Rig GameObject as a child of this root _(ex: Rig Setup)_. To enable animation rigging, the Rig GameObject must have a Rig component. To then connect the Rig to the Animator, you must assign this Rig component to a Rig Layer on the Rig Builder component.

![Rig Setup](images/rig_setup/rig_setup.gif)

After you connect the Rig GameObject to the Rig Builder component, you can add rig elements (i.e., GameObjects with Constraint components) under the Rig GameObject hierarchy. Different rig elements can be organized and hierarchically structured to accommodate any rigging requirement.  

Source objects for Constraints, such as Target Effectors or Hint Effectors, can be placed under their associated Rig GameObject. In the following illustration, _Left Leg IK_ has a **Two Bone IK Constraint** component and is acting as a rig element. It is the parent of both the _LeftFootEffector_ and _LeftLegHint_ source objects.  

![Source Object Example](images/rig_setup/source_object_example.png) 


### Rig Builder Component

The Rig Builder component lives alongside the Animator component and creates a Playable Graph that is appended to the existing Animator state machine.
The Rig Builder component needs to be added to the GameObject that has the Animator component.  Rig Builder needs to affect the same hierarchy as the Animator.

![Rig Builder Setup](images/rig_builder/rig_builder_setup.gif)

Using Rig Layers, the Rig Builder component allows for stacking of multiple Rigs that can be enabled/disabled at any time.

![Rig Builder Layers](images/rig_builder/rig_builder_layers.gif)


### Rig Component

The Rig component is the main entry point for all Constraints in a given control rig.
Note there should only be one Rig component per control rig hierarchy, which you must assign to a Rig Layer on the Rig Builder component.
To use multiple control rigs with a single hierarchy, you must assign each Rig to a different Rig Layer, and enable/disable them independently.

From a technical standpoint, the main purpose of a Rig component is to collect all Constraint components in its local hierarchy.
The component generates an ordered list of _IAnimationJobs_, which are applied to the Animator's pose after its normal evaluation.
The Rig component gathers constraints using _GetComponentsInChildren_, and so the order of components in the Rig's hierarchy determines the evaluation order of the jobs.
This method follows depth-first traversal as shown below:

![Rig Constraint Evaluation Order](images/rig/eval_order.png)

In other words, grouping constraints under a GameObject allows you to manage the evaluation order of these constraints
by modifying the hierarchy.

Control rig hierarchies should hold all the necessary rig elements such as effectors, constraints, and other objects/elements required by the constraint definitions.
The root of a control rig hierarchy should be at the same level as the skeleton root, both under the Game Object holding the Animator.
_In other words, it should not be in the skeleton hierarchy, but rather live beside it._

![Rig Setup](images/rig/rig_setup.gif)

![Rig Weight](images/rig/rig_weight.gif)


Rig components, like all Constraint components, have a Weight property that can be used, animated, and scripted to enable/disable
or ease-in/ease-out an entire control rig hierarchy.

## Other Utilities

### Bone Renderer Component

The Bone Renderer component allows you to define a transform hierarchy to be drawn as bones for visualization and selection during the rigging process.
These bones are not visible in the Game view. This allows you to define your character deformation skeletons for rigging purposes.

![Bone Renderer Setup](images/bone_renderer/bone_renderer_setup.gif)
![Bone Renderer Component](images/bone_renderer/bone_renderer_component.gif)

You can customize the appearance of the bones by modifying Bone Size, Shape and Color.
The package provides these default shapes: Line, Pyramid, and Box.
You can also display tripods of local axes and adjust their size to suit your preference.

![Bone Look Pyramid](images/bone_renderer/bone_looks.png)

### Rig Effectors

Similarly to bones, Rig Effectors allow you to add gizmos to transforms for visualization and selection.
You can add them to any transform in the same hierarchy as a Rig Builder or Rig component.
Like bones, Effectors are not visible in the Game view.
A special Scene View overlay allows you to manage and customize effectors in the Rig hierarchy.

![Rig Effector Overlay](images/rig_effector/rig_effector_setup.gif)

You can also customize the appearance of the effectors by modifying Effector Size, Shape, Color, Offset Position and Offset Rotation.
The shape can be any Mesh asset available in the project.
You can create, delete, or edit multiple effectors at once.

![Rig Effector Shapes](images/rig_effector/rig_effector_shapes.png)


### Rig Transform

When a specific GameObject in your rig's hierarchy is important for manipulation but not referenced by any rig constraints, you must add a **RigTransform** component, found under _Animation Rigging/Setup_.
As shown in the video below, in order to manipulate both the left and right foot IK targets (_lfik_ and _rfik_) of the _2BoneIK_ sample using
their parent transform (_ik_ ), you must add this component to get the expected behavior.

![Rig Transform](images/rig_transform/rig_transform_manipulation.gif)
