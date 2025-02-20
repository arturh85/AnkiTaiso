class_name WallRunToJumpTransition extends MachineTransition


func on_transition():
	if from_state is FirstPersonControllerWallRun and to_state is FirstPersonControllerJump:
		to_state.jump_horizontal_boost = from_state.jump_horizontal_boost
		to_state.jump_vertical_boost = from_state.jump_vertical_boost
	
