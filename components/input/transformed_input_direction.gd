class_name TransformedInputDirection

const MOVE_RIGHT = "move_right"
const MOVE_LEFT = "move_left"
const MOVE_FORWARD = "move_forward"
const MOVE_BACK = "move_back"

var actor: Node
var deadzone := 0.5:
	set(value):
		deadzone = clamp(value, 0.0, 1.0)

var input_direction: Vector2
var input_direction_deadzone_square_shape: Vector2
var input_direction_horizontal_axis: float
var input_direction_vertical_axis: float
var input_direction_horizontal_axis_applied_deadzone: float
var input_direction_vertical_axis_applied_deadzone: float
var world_coordinate_space_direction: Vector3

var previous_input_direction: Vector2
var previous_input_direction_deadzone_square_shape: Vector2
var previous_input_direction_horizontal_axis: float
var previous_input_direction_vertical_axis: float
var previous_input_direction_horizontal_axis_applied_deadzone: float
var previous_input_direction_vertical_axis_applied_deadzone: float
var previous_world_coordinate_space_direction: Vector3


func _init(_actor: Node, _deadzone: float = deadzone):
	assert(_actor is Node2D or _actor is Node3D, "TransformedInputDirection: The actor needs to inherit from Node2D or Node3D to retrieve the input correctly")
	
	actor = _actor
	deadzone = _deadzone


func update_input_direction():
	if actor:
		_update_previous_directions()
		# This handles deadzone in a correct way for most use cases. The resulting deadzone will have a circular shape as it generally should.
		input_direction = Input.get_vector(MOVE_LEFT, MOVE_RIGHT, MOVE_FORWARD, MOVE_BACK)
		
		if actor is Node3D:
			world_coordinate_space_direction = actor.transform.basis * Vector3(input_direction.x, 0, input_direction.y).normalized()
		
		input_direction_deadzone_square_shape = Vector2(
			Input.get_action_strength(MOVE_RIGHT) - Input.get_action_strength(MOVE_LEFT),
			Input.get_action_strength(MOVE_BACK) - Input.get_action_strength(MOVE_FORWARD)
		).limit_length(deadzone)	
		
		input_direction_horizontal_axis = Input.get_axis(MOVE_LEFT, MOVE_RIGHT)
		input_direction_vertical_axis = Input.get_axis(MOVE_FORWARD, MOVE_BACK)
		
		input_direction_horizontal_axis_applied_deadzone = input_direction_horizontal_axis * (1.0 - deadzone)
		input_direction_vertical_axis_applied_deadzone = input_direction_vertical_axis * (1.0 - deadzone)


func _update_previous_directions():
	previous_input_direction = input_direction
	previous_world_coordinate_space_direction = world_coordinate_space_direction
		
	previous_input_direction_deadzone_square_shape = input_direction_deadzone_square_shape
		
	previous_input_direction_horizontal_axis = input_direction_horizontal_axis
	previous_input_direction_vertical_axis = input_direction_vertical_axis
		
	previous_input_direction_horizontal_axis_applied_deadzone = input_direction_horizontal_axis_applied_deadzone
	previous_input_direction_vertical_axis_applied_deadzone = input_direction_vertical_axis_applied_deadzone
