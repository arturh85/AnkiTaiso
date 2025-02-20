@icon("res://components/movement/2D/rotation/orbit_component_2d.svg")
class_name OrbitComponent2D extends Node2D

signal started
signal stopped

@export var rotation_reference: Node2D
@export var radius := 40.0
@export var angle_in_radians = PI / 4
@export var angular_velocity = PI / 2


var active := false:
	set(value):
		if value != active:
			if value:
				started.emit()
			else:
				stopped.emit()
				
		active = value


func _process(delta):
	if active:
		orbit(delta)
		
		
func orbit(delta):
	if rotation_reference:
		active = true
		angle_in_radians += delta * angular_velocity
		angle_in_radians %= TAU
		
		var offset := Vector2(cos(angle_in_radians), sin(angle_in_radians)) * radius
		position = rotation_reference.position + offset
		

func stop():
	active = false
