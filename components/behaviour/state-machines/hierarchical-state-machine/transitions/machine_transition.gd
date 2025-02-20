class_name MachineTransition

var from_state: MachineState
var to_state: MachineState

func should_transition() -> bool:
	return true
	
func on_transition():
	pass
