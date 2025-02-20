class_name WalkToRunTransition extends MachineTransition


func should_transition() -> bool:
	if from_state is FirstPersonControllerWalk and to_state is FirstPersonControllerRun:
		return from_state.actor.run and from_state.catching_breath_timer.is_stopped()
		
	return false
