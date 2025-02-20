class_name AnyToJumpTransition extends MachineTransition


func should_transition():
	if to_state is FirstPersonControllerJump:
		return to_state.actor.jump
			
	return false
	
