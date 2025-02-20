@icon("res://components/camera/2D/shake/trauma_detector.svg")
class_name TraumaDetector3D extends Area3D

@export var camera: ShakeCamera3D

func _ready():
	monitoring = false
	monitorable = true
	collision_layer = 32 ## Shakeable layer
	collision_mask = 0 


func add_trauma(amount: float, time: float):
	if camera:
		camera.add_trauma(amount, time)


func enable():
	set_deferred("monitorable", true)
	

func disable():
	set_deferred("monitorable", false)
