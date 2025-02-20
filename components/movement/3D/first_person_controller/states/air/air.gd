class_name FirstPersonControllerAirState extends MachineState

@export var actor: FirstPersonController
@export var gravity_force := 15.0
@export var jump_input_action := "jump"

@onready var wall_detector = actor.get_node("WallDetector3D") as WallDetector3D


func _ready():
	assert(actor is FirstPersonController, "FirstPersonControllerAirState: This states needs an actor to apply the movement changes")
	
	
func apply_gravity(force: float = gravity_force, delta: float = get_physics_process_delta_time()):
	actor.velocity += VectorWizard.up_direction_opposite_vector3(actor.up_direction) * force * delta


func move(speed: float, acceleration: float = 0, friction: float = 0):
	var world_coordinate_space_direction := actor.transformed_input.world_coordinate_space_direction
	
	if world_coordinate_space_direction.is_zero_approx():
		if friction > 0:
			actor.velocity = actor.velocity.move_toward(Vector3(0, actor.velocity.y, 0), get_physics_process_delta_time() * friction)
		else:
			actor.velocity = Vector3(0, actor.velocity.y, 0)
	else:
		if acceleration > 0:
			actor.velocity = actor.velocity.move_toward(
				Vector3(world_coordinate_space_direction.x * speed, actor.velocity.y, world_coordinate_space_direction.z * speed),
				 	get_physics_process_delta_time() * acceleration
				)
		else:
			actor.velocity = Vector3(world_coordinate_space_direction.x * speed, actor.velocity.y, world_coordinate_space_direction.z * speed)
		
	
func detect_wall_collision():
	if actor.wall_run and wall_detector.active and wall_detector.any_side_is_colliding():
		FSM.change_state_to("WallRun")


func detect_jump() -> void:
	if actor.jump and InputMap.has_action(jump_input_action) and Input.is_action_just_pressed(jump_input_action):
		FSM.change_state_to("Jump")
			
