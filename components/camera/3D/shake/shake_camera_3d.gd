@icon("res://components/camera/2D/shake/shake_camera.svg")
class_name ShakeCamera3D extends Camera3D

@export var trauma_reduction_rate := 1.0
@export var max_rotation_x := 10.0
@export var max_rotation_y := 10.0
@export var max_rotation_z := 5.0
@export var noise_speed := 50.0
@export var noise: FastNoiseLite

var trauma := 0.0
var time := 0.0

var trauma_timer: Timer
var initial_rotation: Vector3
var current_trauma := 0.0 ## This variable works to keep adding trauma while the timer is active


func _ready():
	set_process(false)
	_create_trauma_timer()
	initial_rotation = rotation_degrees


func _process(delta):
	time += delta
	
	if trauma == 0 or (sign(trauma_reduction_rate) < 0 and trauma == 1.0):
		trauma_reduction_rate *= -1
		
		if sign(trauma_reduction_rate) > 0:
			add_trauma(current_trauma)
		else:
			add_trauma(max(0.1, 1.0 - current_trauma))

	
	if trauma_timer.time_left > 0:
		trauma = clamp(trauma - delta * trauma_reduction_rate, 0.0, 1.0)
		
		var intensity = shake_intensity()
		
		rotation_degrees = Vector3(
			initial_rotation.x + max_rotation_x * intensity * get_noise_from_seed(0), 
			initial_rotation.y + max_rotation_y * intensity * get_noise_from_seed(1), 
			initial_rotation.z + max_rotation_z * intensity * get_noise_from_seed(2)
		)
	else:
		finish_trauma()

	
func add_trauma(amount: float, _time: float = 0.0):
	trauma = clamp(trauma + amount, 0.0, 1.0)
	current_trauma = trauma
	
	if _time > 0 and is_instance_valid(trauma_timer):
		trauma_timer.start(_time)
	
	set_process(true)


func shake_intensity() -> float:
	return trauma * trauma
	
	
func get_noise_from_seed(_seed: int) -> float:
	noise.seed = _seed
	
	return noise.get_noise_1d(time * noise_speed)
	

func finish_trauma():
	time = 0.0
	current_trauma = 0.0
	trauma = 0.0
	
	var tween = create_tween()
	tween.tween_property(self, "rotation_degrees", initial_rotation, 0.3).set_ease(Tween.EASE_OUT_IN).set_trans(Tween.TRANS_QUAD)
	
	set_process(false)
	
	
func _create_trauma_timer():
	if trauma_timer == null:
		trauma_timer = Timer.new()
		trauma_timer.name = "TraumaTimer"
		trauma_timer.process_callback = Timer.TIMER_PROCESS_IDLE
		trauma_timer.autostart = false
		trauma_timer.one_shot = true
		
		add_child(trauma_timer)
		trauma_timer.timeout.connect(on_trauma_timer_timeout)


func on_trauma_timer_timeout():
	finish_trauma()