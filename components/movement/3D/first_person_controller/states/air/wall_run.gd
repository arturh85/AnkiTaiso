class_name FirstPersonControllerWallRun extends FirstPersonControllerAirState

## The rotation angle to apply when side wall running
@export var camera_rotation_angle := 0.15
## The opposite force to keep the character sticked to the wall
@export var wall_normal_force := 0.5
## The gravity override to apply in the wall running state
@export var wall_gravity := 3.5
## The climb speed when detection is made on front wall
@export var climb_speed := 6.0
## The climb force when top raycast is not colliding so ledge is reached
@export var climb_ledge_jump := Vector3(0, 2.5, -1.5)
## The speed of side wall running
@export var side_speed := 7.5
## The horizontal boost to apply when jumping from a wall
@export var jump_horizontal_boost := 0
## The vertical boost to apply when jumping from a wall
@export var jump_vertical_boost := 0

@onready var eyes: Node3D = actor.get_node("%Eyes")

## WallDetector class from WallDetector3D
var current_wall_detector
## Save the previous momentum force before wall running to not lose the sense of speed
var momentum: Vector3


func _enter():
	current_wall_detector = wall_detector.current_wall()
	actor.velocity.y = 0
	momentum = Vector3(0, actor.velocity.y, actor.velocity.z)
	
	swing_head()
	
	
func _exit(_next_state):
	current_wall_detector.normal = Vector3.ZERO
	reset_swing_head_rotation()
	

func physics_update(delta):
	apply_opposite_wall_normal_force()
	apply_gravity(wall_gravity, delta)
	apply_forward_movement()

	if actor.is_on_floor():
		if actor.transformed_input.world_coordinate_space_direction.is_zero_approx():	
			FSM.change_state_to("Idle")
		else:
			FSM.change_state_to("Walk")
	
	if actor.wall_jump:
		detect_jump()
		
	if not wall_detector.any_side_is_colliding():
		FSM.change_state_to("Fall")
		
	detect_ledge()	
		
	actor.move_and_slide()


func detect_ledge():
	if not is_wall_side_run() and wall_detector.is_colliding_bottom_front() and not wall_detector.is_colliding_top_front():
		actor.velocity = climb_ledge_jump
	
	
func is_wall_side_run() -> bool:
	return not current_wall_detector.wall_direction.is_equal_approx(Vector3.FORWARD)


func apply_forward_movement():
	if is_wall_side_run():
		var forward_direction = -actor.transform.basis.z.normalized()
		actor.velocity.z = side_speed * forward_direction.z
		actor.velocity += momentum
	else:
		actor.velocity.y += climb_speed
		actor.velocity.y = clamp(actor.velocity.y, 0.1, climb_speed)


func apply_opposite_wall_normal_force():
	if not current_wall_detector.normal.is_zero_approx():
		if is_wall_side_run():
			actor.velocity *= -current_wall_detector.normal * wall_normal_force
		else:
			actor.velocity *= actor.transform.basis.z.normalized() * wall_normal_force


func swing_head():
	if is_wall_side_run():
		var tween = create_tween()
		tween.tween_property(eyes, "rotation:z", sign(current_wall_detector.wall_direction.x) * camera_rotation_angle, 0.3)\
			.set_ease(Tween.EASE_IN).set_trans(Tween.TRANS_CUBIC)


func reset_swing_head_rotation():
	var tween = create_tween()
	tween.tween_property(eyes, "rotation:z", 0, 0.3)\
		.set_ease(Tween.EASE_OUT).set_trans(Tween.TRANS_CUBIC)
		
