@icon("res://components/movement/3D/first_person_controller/effects/first_person_camera_rotator.svg")
class_name FirstPersonCameraMouseRotator extends Node3D

@export var actor: FirstPersonController
@export var eyes: Node3D
@export_range(0.0, 1.0, 0.01) var camera_sensitivity := 0.45
@export_range(0.01, 20.0, 0.01) var mouse_sensitivity := 3.0
@export_range(0, 360, 0.001) var camera_rotation_degrees_limit := 90


func _unhandled_input(event):
	if not actor.locked and Input.mouse_mode == Input.MOUSE_MODE_CAPTURED and event is InputEventMouseMotion:
		rotate_camera(event.relative.x, event.relative.y)


func rotate_camera(relative_x: float, relative_y: float):
	var mouse_sens: float = mouse_sensitivity / 1000.0
	
	var twist_input := relative_x * mouse_sens
	var pitch_input := relative_y * mouse_sens
	
	var camera_rotation_limit = deg_to_rad(camera_rotation_degrees_limit)
	
	var actor_rotation_y = actor.rotation.y - twist_input ## Body rotation
	var actor_rotation_x = eyes.rotation.x - pitch_input ## Head & Neck rotation
	actor_rotation_x = clamp(actor_rotation_x, -camera_rotation_limit, camera_rotation_limit)
	
	actor.rotation.y = lerp_angle(actor.rotation.y, actor_rotation_y, camera_sensitivity)
	eyes.rotation.x = lerp_angle(eyes.rotation.x, actor_rotation_x, camera_sensitivity)
	
	# After relative transforms, camera needs to be renormalized.
	if eyes is Camera3D:
		eyes.orthonormalize()


func lock():
	set_process_unhandled_input(false)


func unlock():
	set_process_unhandled_input(true)
