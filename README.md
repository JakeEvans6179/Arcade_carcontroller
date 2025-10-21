Unity game engine arcade car controller

How to use:
Create an empty gameobject called car and add rigidbody components, player input component as well as the Arcade_car_controller to it.
Create a child gameobject to the car gameobject which will act as the body of the car
Create 4 empty gameobjects as "suspension points" at each wheel position as child gameobjects to the car, 
Additionally, add 4 wheel gameobjects as children to the car at the same position for visualisation (cosmetic) purposes
In the parent gameobject drag the 4 suspension points into the "Wheel Transforms" array in the inspector, drag the cosmetic wheels into the "Wheel Objects" array below

For the player input component create actions OnMove (A vector 2 control type action), OnBrake (Button action) and OnDrift (Button action) setting the binding interaction to press and release for both Button actions.



Design:
Design is based on a rigidbody with the 4 wheel transformations acting as suspension points by being the source of raycasts. If the raycast is within maxSuspensionLength of the ground, an upward force is applied at the wheel point.
This upwards force increases the closer the car is to the ground and so acts like a suspension system

The OnDrift button is used to activate drift mode, when the car is not in drift mode the rigidbody has a sideways counterforce applied at all times to ensure the vehicle doesn't slide.
Once the OnDrift button is pressed the counterforce is removed allowing the car to drift freely.
