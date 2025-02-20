@icon("res://components/interaction/interactable_interactor/interactor_3d.svg")
class_name Interactor3D extends RayCast3D

@export var input_action := "interact"

var current_interactable: Interactable3D
var focused := false
var interacting := false


func _enter_tree():
	enabled = true
	exclude_parent = true
	collision_mask = 8 ## interactables
	collide_with_areas = true
	collide_with_bodies = false


func _unhandled_input(_event: InputEvent):
	if InputMap.has_action(input_action) && Input.is_action_just_pressed(input_action) and current_interactable and not interacting:
		interact(current_interactable);
	

func _physics_process(_delta):
	var detected_interactable: Interactable3D = get_collider() as Interactable3D if is_colliding() else null
	
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
			enabled = false
			
		if interactable.has_signal("interacted"):
			interactable.interacted.emit(self)
	
	
func cancel_interact(interactable: Interactable3D = current_interactable):
	interacting = false
	
	if interactable:
		if interactable.is_scannable():
			enabled = true
			
		if interactable.has_signal("canceled_interaction"):
			interactable.canceled_interaction.emit(self)


func focus(interactable: Interactable3D):
	current_interactable = interactable
	
	if !focused and interactable.has_signal("focused"):
		interactable.focused.emit(self)
	
	focused = true
	
	
func unfocus(interactable: Interactable3D = current_interactable):
	if focused and interactable.has_signal("unfocused"):
		interactable.unfocused.emit(self)
	
	current_interactable = null
	focused = false

