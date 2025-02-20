@icon("res://components/behaviour/state-machines/hierarchical-state-machine/icon.png")
class_name FiniteStateMachine extends Node

signal state_changed(from_state: MachineState, state: MachineState)
signal state_change_failed(from: MachineState, to: MachineState)
signal stack_pushed(new_state: MachineState, stack: Array[MachineState])
signal stack_flushed(stack: Array[MachineState])

@export var current_state: MachineState
@export var enable_stack := true
@export var stack_capacity := 3
@export var flush_stack_when_reach_capacity := false

var states: Dictionary = {}
var states_stack: Array[MachineState] = []
var next_state: MachineState

var transitions := {}
var locked := false


func _ready():
	assert(current_state is MachineState, "FiniteStateMachine: This Finite state machine does not have an initial state defined")
	
	state_changed.connect(on_state_changed)
	
	if get_child_count() > 0:
		_prepare_states()
		enter_state(current_state)
	

func _unhandled_input(event):
	current_state.handle_input(event)


func _physics_process(delta):
	current_state.physics_update(delta)


func _process(delta):
	current_state.update(delta)


func change_state_to(_next_state):
	var error_msg := "FiniteStateMachine: The change of state cannot be done because %s does not exits in this Finite State Machine"
	
	if typeof(_next_state) == TYPE_STRING:
		if not current_state_is_by_name(_next_state) and states.has(_next_state):
			run_transition(current_state, states[_next_state])
			return
		else:
			push_error(error_msg % _next_state)
	
	if _next_state is MachineState:
		if not current_state_is(_next_state) and states.values().has(_next_state):
			run_transition(current_state, _next_state)
			return
		else:
			push_error(error_msg % _next_state.name)
	

func run_transition(from: MachineState, to: MachineState):
	var transition_name = _build_transition_name(from, to)

	if not transitions.has(transition_name):
		transitions[transition_name] = NeutralMachineTransition.new()
	

	var transition := transitions[transition_name] as MachineTransition
	transition.from_state = from
	transition.to_state = to
	
	if transition.should_transition():
		transition.on_transition()
		state_changed.emit(from, to)
		return
	
	state_change_failed.emit(from, to)

## Example register_transition("WalkToRun", WalkToRun.new())
func register_transition(transition_name: String, transition: MachineTransition):
	transitions[transition_name] = transition
	

func enter_state(state: MachineState):
	state.entered.emit()
	state._enter()
	

func exit_state(state: MachineState, _next_state: MachineState):
	state.finished.emit(_next_state)
	state._exit(_next_state)


func current_state_is_by_name(state: String) -> bool:
	return current_state.name.to_lower() == state.to_lower()


func current_state_is(state: MachineState) -> bool:
	return current_state.name.to_lower() == state.name.to_lower()


func current_state_is_not(_states: Array = []) -> bool:
	return _states.any(func(state):
		if typeof(state) == TYPE_STRING:
			return current_state_is_by_name(state)
		
		if state is MachineState:
			return current_state_is(state)
		
		return false
	)
	

func last_state() -> MachineState:
	return states_stack.back() if states_stack.size() > 0 else null


func _build_transition_name(from: MachineState, to: MachineState):
	return "%sTo%sTransition" % [from.name, to.name]
	

func push_state_to_stack(state: MachineState) -> void:
	if enable_stack and stack_capacity > 0:
		if states_stack.size() >= stack_capacity:
			if flush_stack_when_reach_capacity:
				stack_flushed.emit(states_stack)
				states_stack.clear()
			else:
				states_stack.pop_front()
			
		states_stack.append(state)
		stack_pushed.emit(state, states_stack)
			


func lock_state_machine():
	set_process(false)
	set_physics_process(false)
	set_process_input(false)
	set_process_unhandled_input(false)

	
func unlock_state_machine():
	set_process(true)
	set_physics_process(true)
	set_process_input(true)
	set_process_unhandled_input(true)

		
func _prepare_states(node: Node = self):
	for child in node.get_children(true):
		if child is MachineState:
			_add_state_to_dictionary(child)
		else:
			if child.get_child_count() > 0:
				_prepare_states(child)


func _add_state_to_dictionary(state: MachineState):
	if state.is_inside_tree():
		states[state.name] = get_node(state.get_path())
		state.FSM = self
		state.ready()


func on_state_changed(from: MachineState, to: MachineState):
	push_state_to_stack(from)
	exit_state(from, to)
	enter_state(to)
	
	current_state = to


