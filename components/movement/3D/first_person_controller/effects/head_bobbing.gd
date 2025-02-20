@icon("res://components/movement/3D/first_person_controller/effects/head_effect.svg")
class_name HeadBobbingEffect extends Node3D

@export var actor: FirstPersonController
@export var eyes: Node3D
@export var amplitude := 0.08
@export var frequency := 2.0

var bob_time_passed := 0.0
var original_transform_origin: Vector3


func _ready():
	if eyes:
		original_transform_origin = eyes.transform.origin
	
	
func apply(velocity: Vector3, delta: float):
	if velocity.is_zero_approx():
		eyes.transform.origin.move_toward(original_transform_origin, delta * 10.0)
	else:
		bob_time_passed += delta * velocity.length() * float(actor.is_on_floor())
		eyes.transform.origin = Vector3(
				cos(bob_time_passed * frequency / 2.0) * amplitude, 
				(sin(bob_time_passed * frequency) * amplitude), 
				eyes.transform.origin.z
			)
