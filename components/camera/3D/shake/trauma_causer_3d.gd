@icon("res://components/camera/2D/shake/trauma_causer.svg")
class_name TraumaCauser3D extends Area3D

@export_range(0, 1, 0.01) var trauma_amount := 0.5
@export var trauma_time := 1.0

func _ready():
	monitoring = true
	monitorable = false
	collision_layer = 0
	collision_mask = 32 ## Shakeable layer


func cause_trauma():
	for trauma_detector: TraumaDetector3D in get_overlapping_areas().filter(func(area): return area is TraumaDetector3D):
		trauma_detector.add_trauma(trauma_amount, trauma_time)
