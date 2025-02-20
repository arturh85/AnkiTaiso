class_name FirstPersonControllerJump extends FirstPersonControllerAirState

@export var shorten_jump_on_input_release := true
@export var velocity_barrier_to_detect_as_fall := 7.5
@export var air_control_speed := 6.5
@export var air_control_acceleration := 25
@export var air_control_friction := 0.0
@export var jump_times := 1
@export var height_reduced_by_jump = 0.2
@export var jump_height := 8.0:
	set(value):
		jump_height = max(0, value)
		if is_node_ready():
			jump_velocity = _calculate_jump_velocity(jump_height, jump_time_to_peak)
@export var jump_time_to_peak := 0.75:
	set(value):
		jump_time_to_peak = value
		if is_node_ready():
			jump_gravity = _calculate_jump_gravity(jump_height, jump_time_to_peak)
@export var jump_time_to_fall := 0.6:
	set(value):
		jump_time_to_fall = value
		if is_node_ready():
			fall_gravity = _calculate_fall_gravity(jump_height, jump_time_to_fall)
			
@export var override_jump_gravity = 0.0
@export var override_fall_gravity = 0.0

var jump_velocity := 0.0
var jump_gravity := 0.0
var fall_gravity := 0.0

var jump_count := 1
var jump_horizontal_boost := 0
var jump_vertical_boost := 0


func _enter():
	wall_detector.disable_detection_temporary()
	
	jump_velocity = _calculate_jump_velocity(jump_height, jump_time_to_peak)
	jump_gravity = _calculate_jump_gravity(jump_height, jump_time_to_peak)
	fall_gravity = _calculate_fall_gravity(jump_height, jump_time_to_fall)
	
	actor.velocity = Vector3(actor.velocity.x, jump_velocity + jump_vertical_boost, actor.velocity.z + jump_horizontal_boost )
	actor.move_and_slide()
	

func _exit(_next_state):
	jump_count = 1
	jump_horizontal_boost = 0
	jump_vertical_boost = 0
	
	
func physics_update(delta):
	if shorten_jump_on_input_release and Input.is_action_just_released(jump_input_action):
		shorten_jump()
		
	var jump_requested = InputMap.has_action(jump_input_action) and Input.is_action_just_pressed(jump_input_action)
	
	if actor.is_on_floor():
		if actor.transformed_input.input_direction.is_zero_approx():
			FSM.change_state_to("Idle")
		else:
			FSM.change_state_to("Walk")
			
	if actor.velocity.y > 0:
		apply_gravity(jump_gravity, delta)
	else:
		apply_gravity(fall_gravity, delta)
		
		if velocity_barrier_to_detect_as_fall < 0 and actor.velocity.y < velocity_barrier_to_detect_as_fall:
			FSM.change_state_to("Fall")
			
	if jump_requested and jump_times > 1 and jump_count < jump_times:
		actor.velocity.y = _calculate_jump_velocity(jump_height - (jump_times * height_reduced_by_jump), jump_time_to_peak)
		jump_count += 1
	
	move(air_control_speed, air_control_acceleration, air_control_friction)
	
	detect_wall_collision()
	
	actor.move_and_slide()


func shorten_jump():
	var new_jump_velocity = jump_velocity / 2

	if actor.velocity.y > new_jump_velocity:
		actor.velocity.y = new_jump_velocity


func on_wall_detection_timer_timeout():
	wall_detector.active = true


func _calculate_jump_velocity(_jump_height: float = jump_height, time_to_peak: float = jump_time_to_peak) -> float:
	return 2.0 * max(0, _jump_height / (time_to_peak * actor.up_direction.y))


func _calculate_jump_gravity(_jump_height: float = jump_height, time_to_peak: float = jump_time_to_peak) -> float:
	return override_jump_gravity if override_jump_gravity > 0 else 2.0 * max(0, _jump_height) / pow(time_to_peak, 2)


func _calculate_fall_gravity(_jump_height: float = jump_height, time_to_fall: float = jump_time_to_fall) -> float:
	return override_fall_gravity if override_fall_gravity > 0 else 2.0 * max(0, _jump_height) / pow(time_to_fall, 2)
