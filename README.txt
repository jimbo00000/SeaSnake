SeaSnake - locomotion using positional tracking


<- \ 

Forward vector determined by view direction
Lateral motion used to create motion as a propeller through a fluid
TODO: Optional current vector

__Active Motion: Model the head as a fin__
fluidForce = -lateral // fluid force on fin head
finNormal = head right vector // cross look direction with head up vector)
driveForce = reflect(fluidForce, finNormal)
forwardForce = -driveForce

__Passive Motion: Build up momentum then glide and guide__
lookDirection determines preferred direction
Same fin model?

