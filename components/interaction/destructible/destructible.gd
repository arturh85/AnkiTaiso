@icon("res://components/interaction/destructible/destructible.svg")
class_name Destructible extends Node 

signal shards_created
signal destroyed

@export var target: MeshInstance3D:
	set(value):
		if value is MeshInstance3D and is_node_ready():
			target_mesh_faces = value.mesh.get_faces()
			target_material = value.mesh.surface_get_material(0)
			
			thread.start(update_shards_pool)
				
		target = value
		
@export var shards_container: Node3D
@export var group_name := "shards"
@export var shard_type := SHARD_TYPES.BRICK
@export var amount_of_shards := 150
@export var min_shard_size := 0.1
@export var max_shard_size := 0.5
@export var explosion_mode := EXPLOSION_MODES.ALL_DIRECTIONS
@export var min_explosion_power := 4.5
@export var max_explosion_power := 7.0
@export_range(-8, 8, 0.001) var shards_gravity_scale = 0.75
@export_range(0.01, 1000, 0.01) var shard_mass = 0.9


enum SHARD_TYPES {
	BOX,
	BRICK,
}

enum EXPLOSION_MODES {
	ALL_DIRECTIONS,
	HORIZONTAL,
	FORWARD,
	OPPOSITE_TO_CAMERA,
	TO_THE_CAMERA,
	LEFT,
	RIGHT,
	ASCENDANT,
	LANDSLIDE
}

var cached_mesh: Mesh
var target_material: StandardMaterial3D
var target_mesh_faces: PackedVector3Array = []

var rng = RandomNumberGenerator.new()
var pool: Array[Shard] = []

var thread: Thread
	
func _ready():
	destroyed.connect(on_destroyed)
	
	if target:
		target_mesh_faces = target.mesh.get_faces()
		target_material = target.mesh.surface_get_material(0)
		
		thread = Thread.new()
		thread.start(update_shards_pool)


func update_shards_pool():
	pool.clear()
		
	for i in range(amount_of_shards):
		pool.append(Shard.new(group_name))
		
	call_deferred("_shards_loaded")


func _shards_loaded():
	thread.wait_to_finish()
	shards_created.emit()


func destroy():
	if is_instance_valid(target):
		target.hide()
		
		create_shards(amount_of_shards)


func create_shards(amount: int):
	if target:
		for i in range(amount):
			call_thread_safe("create_shard")
			
		destroyed.emit()
	
	
func create_shard():
	var body: Shard = pool.pop_front() as Shard
	
	var mesh := MeshInstance3D.new()
	mesh.scale = generate_random_shard_scale()
	body.add_child(mesh)
	body.continuous_cd = continuos_cd_on_mesh_needs_to_be_applied(mesh)
	create_mesh_shape(mesh)
	
	if target_material:
		mesh.mesh.surface_set_material(0, target_material)
		
	create_mesh_collision(body, mesh)
	
	body.gravity_scale = shards_gravity_scale
	
	if shards_container:
		shards_container.add_child(body)
	else:
		get_tree().root.add_child(body)
		
	var spawn_position := body.position + generate_random_mesh_surface_position(target)
	
	body.global_position = target.global_position + spawn_position
	body.global_rotation = target.global_rotation
	body.linear_damp = 0.1
	body.angular_damp = 0.1
	body.angular_velocity = VectorWizard.generate_3d_random_fixed_direction() * rng.randf_range(0.5, 2.5)
	body.apply_impulse(generate_impulse(explosion_mode), VectorWizard.flip_v3(spawn_position).normalized())
	
	
func generate_impulse(_explosion_mode: EXPLOSION_MODES = explosion_mode) -> Vector3:
	var explosion_power = rng.randf_range(min_explosion_power, max_explosion_power)
	
	match _explosion_mode:
		EXPLOSION_MODES.ALL_DIRECTIONS:
			return VectorWizard.generate_3d_random_direction() * explosion_power
		EXPLOSION_MODES.FORWARD:
			return Vector3.FORWARD * explosion_power
		EXPLOSION_MODES.OPPOSITE_TO_CAMERA:
			return VectorWizard.rotate_horizontal_random(get_viewport().get_camera_3d().global_position.direction_to(target.global_position) * explosion_power)
		EXPLOSION_MODES.TO_THE_CAMERA:
			return -VectorWizard.rotate_horizontal_random(get_viewport().get_camera_3d().global_position.direction_to(target.global_position) * explosion_power)
		EXPLOSION_MODES.HORIZONTAL:
			return [VectorWizard.rotate_horizontal_random(Vector3.LEFT), VectorWizard.rotate_horizontal_random(Vector3.RIGHT)].pick_random() * explosion_power
		EXPLOSION_MODES.LEFT:
			return VectorWizard.rotate_horizontal_random(Vector3.LEFT) * explosion_power
		EXPLOSION_MODES.RIGHT:
			return VectorWizard.rotate_horizontal_random(Vector3.RIGHT) * explosion_power
		EXPLOSION_MODES.ASCENDANT:
			return VectorWizard.rotate_vertical_random(Vector3.UP) * explosion_power
		EXPLOSION_MODES.LANDSLIDE:
			return VectorWizard.rotate_vertical_random(Vector3.DOWN) * explosion_power
		_:
			return VectorWizard.generate_3d_random_direction() * explosion_power
	
	
	
func generate_random_mesh_surface_position(_target: MeshInstance3D) -> Vector3:
	if rng.randf() < 0.1:
		return Vector3.ZERO
	
	return VectorWizard.generate_random_mesh_surface_position(_target)
	

func generate_random_shard_scale():
	var scale: Vector3 = VectorWizard.scale_vector3(Vector3.ONE, rng.randf_range(min_shard_size, max_shard_size))
	
	if shard_type == SHARD_TYPES.BRICK:
		## I reduce the scale of two axis randomly to create brick shards style
		scale = Vector3(scale.x / rng.randf_range(1.1, 10.0), scale.y / rng.randf_range(2.0, 10.0), scale.z)

	return scale
	
		
func create_mesh_collision(body: RigidBody3D, mesh: MeshInstance3D):
	var collision = CollisionShape3D.new()
	collision.shape = BoxShape3D.new()
	collision.shape.size = mesh.scale * 1.25
	
	body.add_child(collision)


func create_mesh_shape(mesh: MeshInstance3D):
	if cached_mesh == null:
		cached_mesh = BoxMesh.new()
	
	mesh.mesh = cached_mesh


func continuos_cd_on_mesh_needs_to_be_applied(mesh: MeshInstance3D):
	return mesh.scale.x < 0.25 and mesh.scale.y < 0.25 and mesh.scale.z < 0.25


func on_destroyed():
	if target:
		target.queue_free()
