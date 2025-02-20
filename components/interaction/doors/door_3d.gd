class_name Door3D extends Node3D

signal opened
signal closed
signal locked
signal unlocked
signal used_wrong_key(wrong_key: PackedScene)

@export var anchor_point: Node3D
@export var interactable: Interactable3D
@export var feedback_label: Label3D
@export var hide_feedback_label_on_ready := true
@export var to_open_text = "open"
@export var to_close_text = "close"
@export var open_angle_degrees := 100.0
@export var close_angle_degrees := 0.0
@export var time_to_open := 0.3
@export var time_to_close = 0.3
@export var delay_before_close = 0.0

@export var is_open := false:
	set(value):
		if value != is_open:
			if value:
				opened.emit()
			else:
				closed.emit()
				
		is_open = value

@export var is_locked := false:
	set(value):
		if value != is_locked:
			if value:
				locked.emit()
			else:
				unlocked.emit()
				
		is_locked = value
		
@export var key: PackedScene


var movement_tween: Tween


func _ready():
	if hide_feedback_label_on_ready and feedback_label:
		feedback_label.hide()
		
	if is_open:
		call_deferred("open")

		
func open():
	if movement_tween == null or (movement_tween and not movement_tween.is_running()):
		movement_tween = create_tween()
		movement_tween.tween_property(anchor_point, "rotation:y", deg_to_rad(open_angle_degrees), time_to_open).set_ease(Tween.EASE_IN)
		movement_tween.tween_callback(func(): is_open = true)
		

func close():
	if movement_tween == null or (movement_tween and not movement_tween.is_running()):
		movement_tween = create_tween()
		movement_tween.tween_property(anchor_point, "rotation:y", deg_to_rad(close_angle_degrees), time_to_close).set_ease(Tween.EASE_OUT)
		movement_tween.tween_callback(func(): is_open = false)
		
## To override
func lock():
	pass

## To override
func unlock():
	pass


func use_key(key_to_use: PackedScene):
	if is_locked:
		if key == key_to_use:
			is_locked =  false
			unlock()
		else:
			is_locked = true
			lock()
		

func on_interacted_door(interactor):
	if is_locked:
		## Write the behaviour when try to open a locked door
		return
		
	if is_open:
		close()
	else:
		open()
	
	
func on_focused_door(interactor):
	if interactor is Interactor3D or interactor is MouseRayCastInteractor:
		if feedback_label:
			feedback_label.show()
			feedback_label.text = to_close_text if is_open else to_open_text


func on_unfocused_door(_interactor):
	if feedback_label:
		feedback_label.hide()
