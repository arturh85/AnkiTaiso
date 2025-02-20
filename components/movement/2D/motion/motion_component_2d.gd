@icon("res://components/movement/2D/motion/motion_component_2d.svg")
class_name MotionComponent2D extends Node2D

signal temporary_speed_started(previous_speed: float, current_speed: float)
signal temporary_speed_finished
signal teleported(from: Vector2, to: Vector2)
signal dashed(position: Vector2)
signal finished_dash(initial_position: Vector2, final_position: Vector2)
signal dash_free_from_cooldown(dash_position: Vector2, current_dash_queue: Array[Vector2])
signal dash_restarted
signal gravity_changed(enabled: bool)
signal inverted_gravity(inverted: bool)
signal temporary_gravity_started(previous_gravity: float, current_gravity: float)
signal temporary_gravity_finished
signal jumped(position: Vector2)
signal wall_jumped(normal: Vector2, position: Vector2)
signal allowed_jumps_reached(jump_positions: Array[Vector2])
signal jumps_restarted
signal coyote_time_started
signal coyote_time_finished

enum PROCESS {
	PROCESS,
	PHYSIC_PROCESS
}

@export var actor: CharacterBody2D
@export var use_process := PROCESS.PHYSIC_PROCESS:
	set(value):
		if value != use_process and is_node_ready():
			set_process(value == PROCESS.PROCESS)
			set_physics_process(value == PROCESS.PHYSIC_PROCESS)
			
		use_process = value
@export_group("Ground")
@export var max_speed := 5.0
@export var acceleration := 1000.0
@export var friction := 400.0

@export_group("Modifiers")
## In seconds, the amount of time a speed modification will endure
@export var default_temporary_speed_time = 3.0

@export_group("Air")
@export var air_control_max_speed := 4.5
@export var air_acceleration := 500.0
@export var air_friction := 250.0
@export var max_fall_velocity := 16.0

@export_group("Gravity")
@export var gravity_force := 9.0
@export var half_gravity_speed_threshold := 10.0

@export_group("Jump")
@export var coyote_time_enabled := true:
	set(value):
		if value != coyote_time_enabled:
			current_coyote_time_frames = coyote_time_grace_frames if value else 0
		
		coyote_time_enabled = value
@export var coyote_time_grace_frames := 25
@export var velocity_barrier_to_detect_as_fall := 7.5
@export var allowed_jump_times := 1
@export var height_reduced_by_jump := 0.2
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

@export_group("Dash")
## The speed multiplier would be applied to the player velocity on runtime
@export var dash_speed_multiplier := 1.5
## The times this character can dash until the cooldown is activated
@export_range(1, 10, 1, "or_greater") var times_can_dash := 1
## The time it takes for the dash ability to become available again.
@export var dash_cooldown := 1.5
## The time this dash will be active
@export var dash_duration := 0.3
## This variable represents whether the character can initiate another dash while already in the dashing state
@export var can_dash_while_dashing : = true

var temporary_speed_timer: Timer

var jump_velocity := 0.0
var jump_gravity := 0.0
var fall_gravity := 0.0
var jump_count := 0
var jump_horizontal_boost := 0
var jump_vertical_boost := 0
var jump_queue: Array[Vector2] = []

var dash_duration_timer: Timer
var dash_queue: Array[Vector2] = []
var is_dashing: bool = false:
	set(value):
		if value != is_dashing:
			if value:
				dashed.emit(actor.global_position)
				
		is_dashing = value

var gravity_enabled: bool = true:
	set(value):
		if value != gravity_enabled:
			gravity_changed.emit(value)
			
		gravity_enabled = value


var is_inverted_gravity: bool = false:
	set(value):
		if value != is_inverted_gravity:
			inverted_gravity.emit(value)
			
		is_inverted_gravity = value

var current_speed: float
var air_current_speed: float
var transformed_input: TransformedInputDirection

var coyote_time_active := false:
	set(value):
		if value != coyote_time_active:
			if not value:
				current_coyote_time_frames = coyote_time_grace_frames
				
		coyote_time_active = value
		
var current_coyote_time_frames := 0

var was_grounded := false
var is_grounded := false:
	set(value):
		if is_grounded != value:
			if value:
				jump_count = 0
				
		is_grounded = value


func _ready():
	assert(actor is CharacterBody2D, "MotionComponent2D: This component needs a CharacterBody2D to apply the motion")
	
	transformed_input = TransformedInputDirection.new(actor)
	current_speed = max_speed
	air_current_speed = air_control_max_speed
	current_coyote_time_frames = coyote_time_grace_frames if coyote_time_enabled else 0
	
	jump_velocity = _calculate_jump_velocity()
	jump_gravity = _calculate_jump_gravity()
	fall_gravity = _calculate_fall_gravity()
	
	_create_dash_duration_timer()
	set_process(use_process == PROCESS.PROCESS)
	set_physics_process(use_process == PROCESS.PHYSIC_PROCESS)


func _process(_delta: float):
	transformed_input.update_input_direction()
	
	was_grounded = is_grounded
	is_grounded = actor.is_on_floor()
	
	update_coyote_time(was_grounded, is_grounded)
	
	
func _physics_process(_delta: float):
	transformed_input.update_input_direction()
	
	was_grounded = is_grounded
	is_grounded = actor.is_on_floor()
	
	update_coyote_time(was_grounded, is_grounded)
	

func update_coyote_time(_was_grounded: bool = was_grounded, _is_grounded: bool = is_grounded):
	if coyote_time_enabled:
		if not coyote_time_active:
			coyote_time_active = _was_grounded and not _is_grounded	
		else:
			current_coyote_time_frames -= 1
			coyote_time_active = current_coyote_time_frames <= 0
		
	

func apply_motion():
	actor.move_and_slide()
	

func apply_motion_with_collide():
	actor.move_and_collide(actor.velocity * _get_current_delta())


func accelerate(delta: float = _get_current_delta()) -> MotionComponent2D:
	var direction := transformed_input.input_direction
	
	if not direction.is_zero_approx():
		if acceleration > 0:
			actor.velocity = actor.velocity.move_toward(direction * current_speed, delta * acceleration)
		else:
			actor.velocity = direction * current_speed
			
	return self
			
			
func decelerate(delta: float = _get_current_delta()) -> MotionComponent2D: 
	if friction > 0:
		actor.velocity = actor.velocity.move_toward(Vector2.ZERO, delta * friction)
	else:
		actor.velocity = Vector2.ZERO
		
	return self


func accelerate_horizontally(delta: float = _get_current_delta()) -> MotionComponent2D:
	var direction := transformed_input.input_direction
	
	if not direction.is_zero_approx():
		if acceleration > 0:
			actor.velocity.x = move_toward(actor.velocity.x, direction.x * current_speed, delta * acceleration)
		else:
			actor.velocity.x = direction.x * current_speed
			
	return self
		
		
func decelerate_horizontally(delta: float = _get_current_delta()) -> MotionComponent2D: 
	if friction > 0:
		actor.velocity.x = move_toward(actor.velocity.x, 0, delta * friction)
	else:
		actor.velocity.x = 0
		
	return self
	
	
func get_jump_gravity() -> float:
	if is_inverted_gravity:
		return jump_gravity if actor.velocity.y > 0 else fall_gravity
	else:
		return jump_gravity if actor.velocity.y < 0 else fall_gravity


func apply_gravity(force: float = gravity_force, delta: float = _get_current_delta()):
	actor.velocity += VectorWizard.up_direction_opposite_vector2(actor.up_direction) * force * delta


func can_jump() -> bool:
	return jump_queue.size() < allowed_jump_times


func jump(height: float = jump_height, bypass: bool = false) -> MotionComponent2D:
	if bypass or can_jump():
		var height_reduced: int =  max(0, jump_queue.size()) * height_reduced_by_jump
		actor.velocity.y = _calculate_jump_velocity(height - height_reduced)
		
		if jump_horizontal_boost > 0:
			actor.velocity.x += sign(actor.velocity.x) + jump_horizontal_boost
		
		if jump_vertical_boost > 0:
			actor.velocity.y += sign(actor.velocity.y) + jump_vertical_boost
		
		
		_add_position_to_jump_queue(actor.global_position)
		jumped.emit(actor.global_position)
		
	return self


func shorten_jump() -> void:
	var actual_velocity_y = actor.velocity.y
	var new_jump_velocity = jump_velocity / 2

	if is_inverted_gravity:
		if actual_velocity_y > new_jump_velocity:
			actor.velocity.y = new_jump_velocity
	else:
		if actual_velocity_y < new_jump_velocity:
			actor.velocity.y = new_jump_velocity


func change_speed_temporary(new_speed: float, time: float = default_temporary_speed_time) -> MotionComponent2D:
	if is_instance_valid(temporary_speed_timer):
		temporary_speed_timer.stop()
		temporary_speed_timer.wait_time = max(0.05, absf(time))
		temporary_speed_timer.start()
		
		current_speed = absf(new_speed)
		
		temporary_speed_started.emit(current_speed, new_speed)

	return self


func has_available_dashes() -> bool:
	return dash_queue.size() < times_can_dash


func can_dash(direction: Vector2 = Vector2.ZERO) -> bool:
	var available_dashes = not direction.is_zero_approx() and has_available_dashes() 
	var while_dashing = can_dash_while_dashing or (not can_dash_while_dashing and not is_dashing)

	return available_dashes and while_dashing


func dash(target_direction: Vector2, speed_multiplier: float = dash_speed_multiplier) -> MotionComponent2D:
	if can_dash(target_direction):
		is_dashing = true
		actor.velocity = target_direction * (current_speed * max(1, absf(speed_multiplier)))

		dash_queue.append(actor.global_position)
		
		if dash_duration_timer.time_left > 0:
			dash_duration_timer.timeout.emit()
		
		dash_duration_timer.start()
	
	return self


func _add_position_to_jump_queue(_position: Vector2):
	jump_queue.append(_position)
	
	if jump_queue.size() == allowed_jump_times:
		allowed_jumps_reached.emit(jump_queue)
	

func _calculate_jump_velocity(_jump_height: float = jump_height, time_to_peak: float = jump_time_to_peak) -> float:
	return 2.0 * max(0, _jump_height / (time_to_peak * actor.up_direction.y))


func _calculate_jump_gravity(_jump_height: float = jump_height, time_to_peak: float = jump_time_to_peak) -> float:
	return override_jump_gravity if override_jump_gravity > 0 else 2.0 * max(0, _jump_height) / pow(time_to_peak, 2)


func _calculate_fall_gravity(_jump_height: float = jump_height, time_to_fall: float = jump_time_to_fall) -> float:
	return override_fall_gravity if override_fall_gravity > 0 else 2.0 * max(0, _jump_height) / pow(time_to_fall, 2)


func _get_current_delta() -> float:
	return get_process_delta_time() if use_process == PROCESS.PROCESS else get_physics_process_delta_time()
	

func _create_temporary_speed_timer(time: float = default_temporary_speed_time) -> void:
	if temporary_speed_timer:
		return
		
	temporary_speed_timer = Timer.new()
	temporary_speed_timer.name = "TemporarySpeedTimer"
	temporary_speed_timer.process_callback = Timer.TIMER_PROCESS_PHYSICS
	temporary_speed_timer.wait_time = time
	temporary_speed_timer.one_shot = true
	temporary_speed_timer.autostart = false

	add_child(temporary_speed_timer)
	temporary_speed_timer.timeout.connect(on_temporary_speed_timer_timeout.bind(max_speed))


func _create_dash_duration_timer(time: float = dash_duration):
	if dash_duration_timer:
		return
		
	dash_duration_timer = Timer.new()
	dash_duration_timer.name = "DashDurationTimer"
	dash_duration_timer.process_callback = Timer.TIMER_PROCESS_PHYSICS
	dash_duration_timer.wait_time = time
	dash_duration_timer.one_shot = true
	dash_duration_timer.autostart = false
	
	add_child(dash_duration_timer)
	dash_duration_timer.timeout.connect(on_dash_duration_timer_timeout)


func _create_dash_cooldown_timer(time: float = dash_cooldown):
	var dash_cooldown_timer: Timer = Timer.new()
	
	dash_cooldown_timer.process_callback = Timer.TIMER_PROCESS_PHYSICS
	dash_cooldown_timer.wait_time = max(0.05, time)
	dash_cooldown_timer.one_shot = true
	dash_cooldown_timer.autostart = true
	
	add_child(dash_cooldown_timer)
	dash_cooldown_timer.timeout.connect(on_dash_cooldown_timer_timeout.bind(dash_cooldown_timer))


func on_temporary_speed_timer_timeout(original_speed: float = max_speed):
	current_speed = original_speed
	temporary_speed_finished.emit()


func on_dash_cooldown_timer_timeout(timer: Timer):
	if not dash_queue.is_empty():
		var last_dash_position = dash_queue.pop_back()
		dash_free_from_cooldown.emit(last_dash_position, dash_queue)
	timer.queue_free()
	
	
func on_dash_duration_timer_timeout():
	is_dashing = false
	_create_dash_cooldown_timer()
	var last_dash = dash_queue.back() if dash_queue.size() else Vector2.ZERO
	finished_dash.emit(last_dash, actor.global_position)
	
