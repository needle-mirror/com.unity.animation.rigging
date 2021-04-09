# Damped Transform

![Example](../images/constraint_damped_transform/damped_transform.gif)

The damped transform constraint allows damping the position and rotation transform values from the source GameObject to the constrained GameObject.
The Maintain Aim option forces the constrained object to always aim at the source object.

![Component](../images/constraint_damped_transform/damped_transform_component.png)

|Properties|Description|
|---|---|
|Weight|The overall weight of the constraint. If set to 0, the constraint has no influence on the Constrained Object. When set to 1, it applies full influence with the current settings. Intermediate values are interpolated linearly.|
|Constrained Object|The GameObject affected by the Source GameObjects.|
|Source|The GameObject that influences the Constrained Object's transform.|
|Damp Position|The weight of positional damping to apply to the Constrained Object. If set to 0, the Constrained Object follows the Source object's position with no damping. If set to 1, the Constrained Object's position is fully damped. As with the constraint's overall weight, intermediate values are interpolated linearly.|
|Damp Rotation|The weight of rotational damping to apply to the Constrained Object. If set to 0, the Constrained Object follows the Source object's rotation with no damping. If set to 1, the Constrained Object's rotation is fully damped. As with the constraint's overall weight, intermediate values are interpolated linearly.|
|Maintain Aim|Specifies whether to maintain the initial rotation offset between the Constrained Object and the Source Object.|
