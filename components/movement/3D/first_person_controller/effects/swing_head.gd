@icon("res://components/movement/3D/first_person_controller/effects/head_effect.svg")
class_name SwingHeadEffect extends Node3D

@export var actor: FirstPersonController
@export var eyes: Node3D
@export_range(0, 360, 0.01) var swing_head_rotation_degrees := 2
@export var swing_head_lerp_factor := 0.05
@export var swing_head_lerp_recovery_factor := 0.15

var original_rotation := 0.0
var allowed_directions := [Vector2.RIGHT, Vector2.LEFT]
var locked := false


func _ready():
	original_rotation = eyes.rotation.z


func apply(direction: Vector2 = Vector2.ZERO):
	if eyes and not locked:
		if direction in allowed_directions:
			eyes.rotation.z = lerp_angle(eyes.rotation.z, -sign(direction.x) * deg_to_rad(swing_head_rotation_degrees), swing_head_lerp_factor)	
		else:
			eyes.rotation.z = lerp_angle(eyes.rotation.z, original_rotation, swing_head_lerp_recovery_factor)	


func lock():
	locked = true


func unlock():
	locked = false
