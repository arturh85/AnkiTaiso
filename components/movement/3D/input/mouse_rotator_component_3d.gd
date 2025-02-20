@icon("res://components/movement/3D/input/3d_rotation_component.svg")
class_name MouseRotatorComponent3D extends Node

@export_range(0.01, 20.0, 0.01) var mouse_sensitivity = 0.3
@export var target: Node3D
@export var input_action: String = "manual_rotate"
@export var reset_rotation_on_release := false

var original_rotation := Vector3.ZERO
var active := false:
	set(value):
		if value != active and is_node_ready():
			if value:
				enable()
			else:
				disable()
				
		active = value


func _ready():
	if target == null:
		target = get_parent() as Node3D
		
	assert(target is Node3D, "MouseRotatorComponent3D: This component needs a Node3D target to apply the rotation")
	
	original_rotation = target.rotation
	
	if active:
		enable()
	else:
		disable()
		
	
func _input(event: InputEvent):
	if target and event is InputEventMouseMotion and InputWizard.action_pressed_and_exists(event, input_action):
		target.rotate_x(event.relative.y * mouse_sensitivity)
		target.rotate_y(event.relative.x * mouse_sensitivity)

	if reset_rotation_on_release and InputWizard.action_just_released_and_exists(input_action):
		reset_target_rotation()
		
	
func reset_target_rotation() -> void:
	if target:
		target.rotation = original_rotation
		
		
func enable():
	active = true
	set_process_input(true)
	
	
func disable():
	active = false
	set_process_input(false)
	reset_target_rotation()
	
	
