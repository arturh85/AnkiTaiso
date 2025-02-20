class_name Shard extends RigidBody3D

var rng := RandomNumberGenerator.new()

var group_name := "shards"

func _init(_group_name: String = group_name):
	group_name = _group_name


func _enter_tree():
	var physics_material = PhysicsMaterial.new()
	physics_material.bounce = rng.randf_range(0.5, 2.0)
	physics_material.friction = rng.randf_range(0.95, 1.0)
	physics_material_override = physics_material
	
	collision_layer = 128 ## Shards layer
	collision_mask = 1 | 2 | 4 | 16
	
	add_to_group(group_name)
	
