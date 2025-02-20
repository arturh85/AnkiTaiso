## https://github.com/JheKWall/Godot-Stair-Step-Demo
@icon("res://components/movement/3D/first_person_controller/mechanics/stair_stepper_3d.svg")
class_name StairStepper3D extends Node3D

@export var actor: FirstPersonController
## Define if the behaviour to step & down stairs it's enabled
@export var stair_stepping_enabled := true:
	set(value):
		if value != stair_stepping_enabled:
			set_physics_process(value)
			stair_stepping = value
		stair_stepping_enabled = value
## Maximum height in meters the player can step up.
@export var max_step_up := 0.5 
## Maximum height in meters the player can step down.
@export var max_step_down := -0.5
## Shortcut for converting vectors to vertical
@export var vertical := Vector3(0, 1, 0)
## Shortcut for converting vectors to horizontal
@export var horizontal := Vector3(1, 0, 1)

var is_grounded := true
var was_grounded := true
var stair_stepping := false


func _physics_process(_delta: float):
	was_grounded = is_grounded
	is_grounded = actor.is_on_floor()
	

func stair_step_up():
	if not stair_stepping_enabled:
		return
		
	stair_stepping = false
	
	if actor.transformed_input.world_coordinate_space_direction.is_zero_approx():
		return
	
	var body_test_params := PhysicsTestMotionParameters3D.new()
	var body_test_result := PhysicsTestMotionResult3D.new()
	var test_transform = actor.global_transform	 ## Storing current global_transform for testing
	var distance = actor.transformed_input.world_coordinate_space_direction * 0.1	## Distance forward we want to check
	
	body_test_params.from =  actor.global_transform ## Self as origin point
	body_test_params.motion = distance ## Go forward by current distance

	# Pre-check: Are we colliding?
	if not PhysicsServer3D.body_test_motion(actor.get_rid(), body_test_params, body_test_result):
		return
	
	## 1- Move test transform to collision location
	var remainder = body_test_result.get_remainder() ## Get remainder from collision
	test_transform = test_transform.translated(body_test_result.get_travel()) ## Move test_transform by distance traveled before collision

	## 2. Move test_transform up to ceiling (if any)
	var step_up = max_step_up * vertical
	
	body_test_params.from = test_transform
	body_test_params.motion = step_up
	
	PhysicsServer3D.body_test_motion(actor.get_rid(), body_test_params, body_test_result)
	test_transform = test_transform.translated(body_test_result.get_travel())

	## 3. Move test_transform forward by remaining distance
	body_test_params.from = test_transform
	body_test_params.motion = remainder
	PhysicsServer3D.body_test_motion(actor.get_rid(), body_test_params, body_test_result)
	test_transform = test_transform.translated(body_test_result.get_travel())


	## 3.5 Project remaining along wall normal (if any). So you can walk into wall and up a step
	if body_test_result.get_collision_count() != 0:
		remainder = body_test_result.get_remainder().length()

		### Uh, there may be a better way to calculate this in Godot.
		var wall_normal = body_test_result.get_collision_normal()
		var dot_div_mag = actor.transformed_input.world_coordinate_space_direction.dot(wall_normal) / (wall_normal * wall_normal).length()
		var projected_vector = (actor.transformed_input.world_coordinate_space_direction - dot_div_mag * wall_normal).normalized()

		body_test_params.from = test_transform
		body_test_params.motion = remainder * projected_vector
		PhysicsServer3D.body_test_motion(actor.get_rid(), body_test_params, body_test_result)
		test_transform = test_transform.translated(body_test_result.get_travel())

	## 4. Move test_transform down onto step
	body_test_params.from = test_transform
	body_test_params.motion = max_step_up * -vertical
	
	## Return if no collision
	if not PhysicsServer3D.body_test_motion(actor.get_rid(), body_test_params, body_test_result):
		return
	
	test_transform = test_transform.translated(body_test_result.get_travel())
	
	## 5. Check floor normal for un-walkable slope
	var surface_normal = body_test_result.get_collision_normal()
	var temp_floor_max_angle = actor.floor_max_angle + deg_to_rad(20)
	
	if (snappedf(surface_normal.angle_to(vertical), 0.001) > temp_floor_max_angle):
		return
	
	stair_stepping = true
	# 6. Move player up
	var global_pos = actor.global_position
	#var step_up_dist = test_transform.origin.y - global_pos.y

	actor.velocity.y = 0
	global_pos.y = test_transform.origin.y
	actor.global_position = global_pos

	
func stair_step_down():
	if not stair_stepping_enabled:
		return
		
	stair_stepping = false
	
	if actor.velocity.y <= 0 and was_grounded:
		## Initialize body test variables
		var body_test_result = PhysicsTestMotionResult3D.new()
		var body_test_params = PhysicsTestMotionParameters3D.new()

		body_test_params.from = actor.global_transform ## We get the player's current global_transform
		body_test_params.motion = Vector3(0, max_step_down, 0) ## We project the player downward

		if PhysicsServer3D.body_test_motion(actor.get_rid(), body_test_params, body_test_result):
			stair_stepping = true
			# Enters if a collision is detected by body_test_motion
			# Get distance to step and move player downward by that much
			actor.position.y += body_test_result.get_travel().y
			actor.apply_floor_snap()
			is_grounded = true
