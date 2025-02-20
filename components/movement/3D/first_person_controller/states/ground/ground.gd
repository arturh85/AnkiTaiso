class_name FirstPersonControllerGroundState extends MachineState

@export var actor: FirstPersonController
@export var gravity_force := 15.0
@export var crouch_input_action := "crouch"
@export var crawl_input_action := "crawl"
@export var run_input_action := "run"
@export var jump_input_action := "jump"

@onready var stair_stepper = actor.get_node("StairStepper3D") as StairStepper3D

var was_grounded := false
var is_grounded := false


func _ready():
	assert(actor is FirstPersonController, "FirstPersonControllerGroundState: This states needs an actor to apply the movement changes")

func physics_update(delta):
	was_grounded = is_grounded
	is_grounded = actor.is_on_floor()
	
	if not is_grounded:
		apply_gravity(gravity_force, delta)
	
	if is_falling():
		FSM.change_state_to("Fall")


func move(speed: float, acceleration: float = 0, friction: float = 0):
	var world_coordinate_space_direction := actor.transformed_input.world_coordinate_space_direction
	
	if world_coordinate_space_direction.is_zero_approx():
		if friction > 0:
			actor.velocity = actor.velocity.move_toward(Vector3.ZERO, get_physics_process_delta_time() * friction)
		else:
			actor.velocity = Vector3.ZERO
	else:
		if acceleration > 0:
			actor.velocity = actor.velocity.move_toward(
				Vector3(world_coordinate_space_direction.x * speed, actor.velocity.y, world_coordinate_space_direction.z * speed),
				 	get_physics_process_delta_time() * acceleration
				)
		else:
			actor.velocity = Vector3(world_coordinate_space_direction.x * speed, actor.velocity.y, world_coordinate_space_direction.z * speed)
	
	
func apply_gravity(force: float, delta: float):
	actor.velocity += VectorWizard.up_direction_opposite_vector3(actor.up_direction) * force * delta



func is_falling() -> bool:
	if actor.is_on_floor() or stair_stepper.stair_stepping:
		return false
		
	var opposite_up_direction := VectorWizard.up_direction_opposite_vector3(actor.up_direction)
	
	var opposite_to_gravity_vector := (opposite_up_direction.is_equal_approx(Vector3.DOWN) and actor.velocity.y < 0) \
		or (opposite_up_direction.is_equal_approx(Vector3.UP) and actor.velocity.y > 0) \
		or (opposite_up_direction.is_equal_approx(Vector3.RIGHT) and actor.velocity.x > 0) \
		or (opposite_up_direction.is_equal_approx(Vector3.LEFT) and actor.velocity.x < 0)
	
	return opposite_to_gravity_vector


func detect_jump() -> void:
	if InputMap.has_action(jump_input_action) and Input.is_action_just_pressed(jump_input_action):
		FSM.change_state_to("Jump")
		
		
func detect_crouch() -> void:
	if actor.crouch and actor.is_on_floor() and InputMap.has_action(crouch_input_action) and Input.is_action_pressed(crouch_input_action):
		FSM.change_state_to("Crouch")


func detect_crawl() -> void:
	if actor.crawl and actor.is_on_floor() and InputMap.has_action(crawl_input_action) and Input.is_action_pressed(crawl_input_action):
		FSM.change_state_to("Crawl")
		
