class_name RunToWalkTransition extends MachineTransition


func on_transition():
	if from_state is FirstPersonControllerRun and to_state is FirstPersonControllerWalk:
		if from_state.in_recovery:
			to_state.catching_breath_timer.start()
	
