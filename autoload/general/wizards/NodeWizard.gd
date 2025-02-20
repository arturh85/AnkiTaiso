class_name NodeWizard


static func global_direction_to_v2(a: Node2D, b: Node2D) -> Vector2:
	return a.global_position.direction_to(b.global_position)


static func local_direction_to_v2(a: Node2D, b: Node2D) -> Vector2:
	return a.position.direction_to(b.position)


static func global_distance_to_v2(a: Node2D, b: Node2D) -> float:
	return a.global_position.distance_to(b.global_position)


static func local_distance_to_v2(a: Node2D, b: Node2D) -> float:
	return a.position.distance_to(b.position)
	
	
static func global_direction_to_v3(a: Node3D, b: Node3D) -> Vector3:
	return a.global_position.direction_to(b.global_position)


static func local_direction_to_v3(a: Node3D, b: Node3D) -> Vector3:
	return a.position.direction_to(b.position)


static func global_distance_to_v3(a: Node3D, b: Node3D) -> float:
	return a.global_position.distance_to(b.global_position)


static func local_distance_to_v3(a: Node3D, b: Node3D) -> float:
	return a.position.distance_to(b.position)


## Only works for native Godot Classes like Area3D, Camera2D, etc.
static func find_nodes_of_class_recursively(node: Node, class_to_find: Node, result: Array = []):
	var childrens = node.get_children(true)
	
	for child in childrens:
		if child.is_class(class_to_find.get_class()):
			result.append(child)
		else:
			find_nodes_of_class_recursively(child, class_to_find, result)

	
## Only works for native Godot Classes like Area3D, Camera2D, etc.
static func first_node_of_class(node: Node, class_to_find: Node):
	if node.get_child_count() == 0:
		return null

	for child in node.get_children():
		if child.is_class(class_to_find.get_class()):
			class_to_find.free()
			return child
	
	class_to_find.free()
	return null
	

static func get_last_child(node: Node):
	var node_count := node.get_child_count()
	
	if node_count == 0:
		return null
		
	return node.get_child(node_count - 1)


static func first_node_in_group(node: Node, group: String):
	if node.get_child_count() == 0 || group.is_empty():
		return null
		
	for child in node.get_children():
		if child.is_in_group(group):
			return child
			
	return null
	

static func remove_and_queue_free_children(node: Node) -> void:
	if node.get_child_count() == 0:
		return
		
	var childrens = node.get_children()
	childrens.reverse()
	
	for child in childrens:
		node.remove_child(child)
		child.queue_free()


static func queue_free_children(node: Node) -> void:
	if node.get_child_count() == 0:
		return
		
	var childrens = node.get_children()
	childrens.reverse()
	
	for child in childrens:
		child.queue_free()


static func set_owner_to_edited_scene_root(node: Node) -> void:
	if Engine.is_editor_hint():
		node.owner = node.get_tree().edited_scene_root
	

static func get_tree_depth(node: Node) -> int:
	var depth := 0
	
	while(node.get_parent() != null):
		depth += 1
		node = node.get_parent()
		
	return depth
	

func get_nearest_node_by_distance(from: Vector2, nodes: Array = [], min_distance: float = 0.0, max_range: float = 9999) -> Dictionary:
	var result := {"target": null, "distance": null}
	
	for node in nodes.filter(func(node): return node is Node2D or node is Node3D): ## Only allows node that can use global_position in the world
		var distance_to_target: float = node.global_position.distance_to(from)
		
		if Utilities.decimal_value_is_between(distance_to_target, min_distance, max_range) and (result.target == null or (distance_to_target < result.distance)):
			result.target = node
			result.distance = distance_to_target
		
		
	return result
	

func get_farthest_node_by_distance(from: Vector2, nodes: Array = [], min_distance: float = 0.0, max_range: float = 9999) -> Dictionary:
	var farthest := {"target": null, "distance": 0.0}
	
	for node in nodes.filter(func(node): return node is Node2D or node is Node3D): ## Only allows node that can use global_position in the world
		var distance_to_target: float = node.global_position.distance_to(from)
		
		if Utilities.decimal_value_is_between(distance_to_target, min_distance, max_range) and (farthest.target == null or (distance_to_target > farthest.distance)):
			farthest.target = node
			farthest.distance = distance_to_target
		
		
	return farthest



func get_nearest_nodes_sorted_by_distance(from: Vector2, nodes: Array = [], min_distance: float = 0.0, max_range: float = 9999) -> Array:
	var nodes_copy = nodes.duplicate()\
		.filter(func(node): return (node is Node2D or node is Node3D) and Utilities.decimal_value_is_between(node.global_position.distance_to(from), min_distance, max_range))
		
	nodes_copy.sort_custom(func(a, b): return a.global_position.distance_to(from) < b.global_position.distance_to(from))
	
	return nodes_copy
	
