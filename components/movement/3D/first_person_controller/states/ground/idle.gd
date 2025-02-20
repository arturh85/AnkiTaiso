class_name FirstPersonControllerIdle extends FirstPersonControllerGroundState


func _enter():
	actor.velocity = Vector3.ZERO
	
	
func physics_update(delta):
	super.physics_update(delta)
	
	if not actor.transformed_input.input_direction.is_zero_approx():
		FSM.change_state_to("Walk")
		
	actor.move_and_slide()
	
	detect_jump()
	detect_crouch()
