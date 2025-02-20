class_name DestructibleMeshInstance extends MeshInstance3D

@export var hurtbox: Hurtbox3D
@export var destructible: Destructible
@export var affected_static_bodies: Array[StaticBody3D] = []
@export var physics_material: PhysicsMaterial

var rng := RandomNumberGenerator.new()

func get_custom_class() -> String:
	return "DestructibleMeshInstance"
	
	
func _ready():
	assert(hurtbox is Hurtbox3D, "DestructibleMeshInstance: This node needs a hurtbox to detect hitboxes that triggers the destroy() method")
	assert(destructible is Destructible, "DestructibleMeshInstance: This node needs a destructible class")
	
	if destructible.target == null:
		destructible.target = self
		
	hurtbox.hitbox_detected.connect(on_hitbox_detected)
	destructible.destroyed.connect(on_destroyed)


func convert_static_body_to_throwable(static_body: StaticBody3D) -> void:
	var throwable = Throwable3D.new()
		
	for child in static_body.get_children():
		child.reparent(throwable)
		child.position = Vector3.ZERO
	
	static_body.get_parent().add_child(throwable)

	throwable.global_position = static_body.global_position

	if physics_material == null:
		physics_material = PhysicsMaterial.new()
		physics_material.bounce = rng.randf_range(0.1, 1.0)
		physics_material.friction = rng.randf_range(0.9, 1.0)
		throwable.physics_material_override = physics_material
		
	throwable.drop()
	
	static_body.queue_free()


### SIGNAL CALLBACKS ###
func on_hitbox_detected(hitbox: Hitbox3D):
	## This is a temporary placeholder to test the destructible, logic can be different
	if hitbox.get_parent() is Throwable3D:
		destructible.destroy()


func on_destroyed():
	for affected_static_body in affected_static_bodies:
		convert_static_body_to_throwable(affected_static_body)

