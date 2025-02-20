class_name FirstPersonControllerFall extends FirstPersonControllerAirState

@export var air_control_speed := 6.5
@export var air_control_acceleration := 25
@export var air_control_friction := 0.0
## Enable coyote time when is falling to allow jump in a specific window frame range
@export var coyote_time := true
## This the time to record the coyote_time on 60 fps, so 15/60 will be 1/4s
@export var coyote_time_frames := 25
## Enable jump_buffer when is falling to allow jump with previous inputs from player
@export var jump_buffer := true
## This the time to record the jump buffer on 60 fps, so 15/60 will be 1/4s
@export var jump_buffer_time_frames := 25 

var current_coyote_time_frames := 0
var current_jump_buffer_frames := 0

var jump_requested := false
var was_grounded := false
var is_grounded := false


func _enter():
	jump_requested = false
	current_coyote_time_frames = coyote_time_frames
	current_jump_buffer_frames = jump_buffer_time_frames


func physics_update(delta):
	was_grounded = is_grounded
	is_grounded = actor.is_on_floor()
	
	apply_gravity(gravity_force, delta)
	
	jump_requested = InputMap.has_action(jump_input_action) and Input.is_action_just_pressed(jump_input_action)
	current_coyote_time_frames -= 1
	current_jump_buffer_frames -= 1
	
	if jump_requested:
		if _coyote_time_count_is_active():
			FSM.change_state_to("Jump")
			
	if (not was_grounded and is_grounded) or actor.is_on_floor():
		if _jump_buffer_count_is_active() and jump_requested:
			FSM.change_state_to("Jump")
		else:
			FSM.change_state_to("Idle")
		
	move(air_control_speed, air_control_acceleration, air_control_friction)
	actor.move_and_slide()


func _coyote_time_count_is_active() -> bool:
	return coyote_time and actor.jump and \
			current_coyote_time_frames > 0 and \
			not FSM.last_state() is FirstPersonControllerJump ## This last check avoid doing unexpected double jump


func _jump_buffer_count_is_active() -> bool:
	return jump_buffer and actor.jump and current_jump_buffer_frames > 0
