@icon("res://components/movement/3D/first_person_controller/mechanics/wall_detector.svg")
class_name WallDetector3D extends Node3D

signal enabled
signal disabled

@export var wall_detection_disabled_time := 0.45
@export var between_separation_from_origin := 0.1
@export_range(0.1, 10.0, 0.01) var right_distance_detector := 0.5
@export_range(0.1, 10.0, 0.01) var left_distance_detector := 0.5
@export_range(0.1, 10.0, 0.01) var front_distance_detector := 0.7
@export var top_front_detector_position := Vector3(0, 0.5, 0)
@export var bottom_front_detector_position :=  Vector3(0, 0.5, 0)


enum WALL_SIDES {
	TOP_FRONT,
	BOTTOM_FRONT,
	RIGHT,
	LEFT,
	BACK
}

var active := false:
	set(value):
		if value != active:
			if value:
				enabled.emit()
			else:
				disabled.emit()
				
		active = value
		
var right_raycast: RayCast3D
var left_raycast: RayCast3D 
var top_front_raycast: RayCast3D
var bottom_front_raycast: RayCast3D

var detectors := {}
var wall_detection_timer: Timer

func _ready():
	_create_wall_raycast_detectors()
	_create_wall_detection_timer()
	

func _physics_process(_delta):
	update_detectors()
	

func current_wall() -> WallDetector:
	var current_detector = detectors.values()\
		.filter(func(detector: WallDetector): return not detector.normal.is_zero_approx())\
		.front()

	return current_detector


func any_side_is_colliding() -> bool:
	return active and \
		(is_colliding_top_front() or is_colliding_bottom_front() or is_colliding_right() or is_colliding_left())


func is_colliding_top_front() -> bool:
	return detectors[WALL_SIDES.TOP_FRONT].is_colliding


func is_colliding_bottom_front() -> bool:
	return detectors[WALL_SIDES.BOTTOM_FRONT].is_colliding


func is_colliding_front() -> bool:
	return is_colliding_top_front() and is_colliding_bottom_front()


func is_colliding_right() -> bool:
	return detectors[WALL_SIDES.RIGHT].is_colliding


func is_colliding_left() -> bool:
	return detectors[WALL_SIDES.LEFT].is_colliding


func enable_detection():
	if not is_physics_processing():
		active = true
		set_physics_process(true)
		
		for detector: WallDetector in detectors.values():
			if detector.raycast is RayCast3D:
				detector.raycast.enabled = true


func disable_detection():
	if is_physics_processing():
		active = false
		set_physics_process(false)
		
		for detector: WallDetector in detectors.values():
			if detector.raycast is RayCast3D:
				detector.raycast.enabled = false


func disable_detection_temporary(time: float = wall_detection_disabled_time):
	if is_instance_valid(wall_detection_timer):
		wall_detection_timer.start(time)
		disable_detection()
	
	
func update_detectors(force_raycast_update: bool = false):
	for detector: WallDetector in detectors.values():
		detector.tick_update(force_raycast_update)
	
	
func _create_wall_raycast_detectors():
	NodeWizard.queue_free_children(self)
	
	right_raycast = RayCast3D.new()
	right_raycast.target_position = Vector3.RIGHT * right_distance_detector
	right_raycast.position = Vector3.RIGHT * between_separation_from_origin
	
	left_raycast = RayCast3D.new()
	left_raycast.target_position = Vector3.LEFT * left_distance_detector
	left_raycast.position = Vector3.LEFT * between_separation_from_origin
	
	top_front_raycast = RayCast3D.new()
	top_front_raycast.target_position = Vector3.FORWARD * front_distance_detector
	top_front_raycast.position = top_front_detector_position
	
	bottom_front_raycast = RayCast3D.new()
	bottom_front_raycast.target_position = Vector3.FORWARD * front_distance_detector
	bottom_front_raycast.position = bottom_front_detector_position
	
	add_child(right_raycast)
	add_child(left_raycast)
	add_child(top_front_raycast)
	add_child(bottom_front_raycast)
	
	detectors = {
		WALL_SIDES.TOP_FRONT: WallDetector.new(top_front_raycast, Vector3.FORWARD),
		WALL_SIDES.BOTTOM_FRONT: WallDetector.new(bottom_front_raycast, Vector3.FORWARD),
		WALL_SIDES.RIGHT: WallDetector.new(right_raycast, Vector3.RIGHT),
		WALL_SIDES.LEFT: WallDetector.new(left_raycast, Vector3.LEFT)
	}


func _create_wall_detection_timer():
	if wall_detection_timer == null:
		wall_detection_timer = Timer.new()
		wall_detection_timer.name = "WallDetectionTimer"
		wall_detection_timer.wait_time = wall_detection_disabled_time
		wall_detection_timer.process_callback = Timer.TIMER_PROCESS_PHYSICS
		wall_detection_timer.autostart = false
		wall_detection_timer.one_shot = true
		
		add_child(wall_detection_timer)
		wall_detection_timer.timeout.connect(on_wall_detection_timer_timeout)


func on_wall_detection_timer_timeout():
	enable_detection()


class WallDetector:
	var wall_direction := Vector3.ZERO
	var raycast: RayCast3D
	var is_colliding := false
	var normal := Vector3.ZERO

	func _init(_raycast: RayCast3D, _wall_direction: Vector3):
		raycast = _raycast
		wall_direction = _wall_direction
		

	func tick_update(force_raycast_update: bool = false):
		if raycast is RayCast3D:
			if force_raycast_update:
				raycast.force_raycast_update()
			
			is_colliding = raycast.is_colliding()
			normal = raycast.get_collision_normal() if raycast.is_colliding() else Vector3.ZERO


	func is_wall_right() -> bool:
		return wall_direction.is_equal_approx(Vector3.RIGHT)


	func is_wall_left() -> bool:
		return wall_direction.is_equal_approx(Vector3.LEFT)


	func is_wall_front() -> bool:
		return wall_direction.is_equal_approx(Vector3.FORWARD)



	func as_dictionary() -> Dictionary:
		return {
			"wall_direction": wall_direction,
			"raycast": raycast,
			"is_colliding": is_colliding,
			"normal": normal
		}
