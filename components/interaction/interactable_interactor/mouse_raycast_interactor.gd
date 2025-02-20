@icon("res://components/interaction/interactable_interactor/interactor_3d.svg")
class_name MouseRayCastInteractor extends Node3D

@export var origin_camera: Camera3D
@export var ray_length := 1000.0
@export var interact_button := MOUSE_BUTTON_LEFT
@export var collision_mask := 8 ## Interactables

var current_interactable: Interactable3D
var focused := false
var interacting := false
var mouse_position: Vector2


func _input(event: InputEvent):
	if origin_camera and event is InputEventMouse:
		mouse_position = event.position
		
		if interact_button == MOUSE_BUTTON_LEFT and InputWizard.is_mouse_left_click(event) \
			or interact_button == MOUSE_BUTTON_RIGHT and InputWizard.is_mouse_right_click(event):
				interact(current_interactable)
				
				
func _ready():
	assert(origin_camera is Camera3D, "MouseRayCast: This node needs a camera to create the mouse raycast")
	
	
func _process(_delta):
	if origin_camera and mouse_mode_is_valid():
		var detected_interactable: Interactable3D = get_detected_interactable()
		
		if detected_interactable:
			if current_interactable != detected_interactable and !focused:
				focus(detected_interactable)
		else:
			if focused and not interacting and current_interactable:
				unfocus(current_interactable)
		

func interact(interactable: Interactable3D):
	if interactable:
		if interactable.is_scannable():
			interacting = true
			
		if interactable.has_signal("interacted"):
			interactable.interacted.emit(self)
	
	
func cancel_interact(interactable: Interactable3D = current_interactable):
	interacting = false
	
	if interactable:
		if interactable.has_signal("canceled_interaction"):
			interactable.interacted.emit(self)


func focus(interactable: Interactable3D):
	current_interactable = interactable
	
	if !focused and interactable.has_signal("focused"):
		interactable.interacted.emit(self)
	
	focused = true
	
	
func unfocus(interactable: Interactable3D = current_interactable):
	if focused and interactable.has_signal("unfocused"):
		interactable.interacted.emit(self)
	
	current_interactable = null
	focused = false


func get_detected_interactable() -> Interactable3D:
	var world_space := get_world_3d().direct_space_state
	var from := origin_camera.project_ray_origin(mouse_position)
	var to := from + origin_camera.project_ray_normal(mouse_position) * ray_length
	
	var ray_query = PhysicsRayQueryParameters3D.create(from, to, collision_mask)
	ray_query.collide_with_areas = true
	ray_query.collide_with_bodies = false
	
	var result := world_space.intersect_ray(ray_query)
	
	if mouse_mode_is_valid() and result.has("collider"):
		return result.collider as Interactable3D
		
	return null


func mouse_mode_is_valid() -> bool:
	return  Input.mouse_mode in [Input.MOUSE_MODE_VISIBLE, Input.MOUSE_MODE_CONFINED]
