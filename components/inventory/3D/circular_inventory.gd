@icon("res://components/inventory/3D/circular_inventory.svg")
class_name CircularInventory3D extends Node3D

signal current_item_changed(item)
signal item_delete_requested(item)
signal item_deleted(item)

@export var max_items = 6
@export var radius = 3: 
	set(value):
		radius = max(0, value)
		if is_node_ready():
			adjust_to_camera(radius, distance_from_camera)

@export var next_item_left_key := KEY_A
@export var next_item_right_key := KEY_D
@export var next_item_spin_duration := 0.3
@export var distance_from_camera := 1.5
@export var items_can_be_deleted := false
@export var item_delete_needs_confirmation := true
@export var delete_item_input_action := "ui_accept"
@onready var camera_3d: Camera3D = $Camera3D
@onready var items_container: Node3D = $Items

var inventory = []
var current_item := {"item": null, "requested_to_delete": false}


func _unhandled_input(event: InputEvent):
	handle_movement(event)
	delete_current_item()
		
	
func _ready():
	var number_of_items = min(max_items, items_container.get_child_count())
	
	for index in number_of_items:
		var angle = deg_to_rad(rad_to_deg(TAU) / number_of_items * index)
		var x = radius * cos(angle)
		var z = radius * sin(angle) 
		
		var child = items_container.get_child(index)
		child.position = Vector3(x, 0, z)
		
		print(child.position)
		if index == 0:
			current_item.item = child
			
		inventory.append(child)
	
	adjust_to_camera()


func adjust_to_camera(_radius: int = radius, _distance_from_camera: float = distance_from_camera):
	items_container.global_position.z = (_radius + _distance_from_camera) * -sign(_radius)


func show_inventory():
	camera_3d.make_current()
	set_process_unhandled_input(true)
	
	
func hide_inventory():
	camera_3d.current = false
	set_process_unhandled_input(false)
	

func next_item(direction: Vector2 = Vector2.ZERO):
	var inventory_size = inventory.size()
	
	if inventory_size > 1:    
		var angleIncrement = rad_to_deg(TAU) / inventory_size
		
		for i in range(inventory_size):
			var item = inventory[i]
			var angle = deg_to_rad(angleIncrement * (i + direction.x))

			var tween = create_tween()
			tween.tween_property(item, "position", Vector3(radius * cos(angle), 0, radius * sin(angle)), next_item_spin_duration)
			
		if direction.is_equal_approx(Vector2.RIGHT):
			rotate_array_radial_right(inventory)
			current_item.item = inventory.back()
			current_item.requested_to_delete = false
			current_item_changed.emit(current_item.item)
		
		if direction.is_equal_approx(Vector2.LEFT):
			rotate_array_radial_left(inventory)
			current_item.item = inventory.back()
			current_item.requested_to_delete = false
			current_item_changed.emit(current_item.item)
			
		

# Rotate the array radially to the right
func rotate_array_radial_right(array):
	var last_element = array.pop_back()
	array.insert(0, last_element)

# Rotate the array radially to the left
func rotate_array_radial_left(array):
	var first_element = array.pop_front()
	array.append(first_element)
	
	
func handle_movement(event: InputEvent):
	var direction := Vector2.ZERO
	
	if event is InputEventKey and event.pressed:
		if event.keycode == next_item_left_key:
			direction = Vector2.LEFT
		elif event.keycode == next_item_right_key:
			direction = Vector2.RIGHT

		if direction in [Vector2.LEFT, Vector2.RIGHT]:
			next_item(direction)
			return
		
	
func delete_current_item():
	if items_can_be_deleted and InputWizard.action_just_pressed_and_exists(delete_item_input_action):
		if current_item.item and item_delete_needs_confirmation and not current_item.requested_to_delete:
			item_delete_requested.emit(current_item.item)
			current_item.requested_to_delete = true
			return
		
		if current_item.item or (current_item.item and item_delete_needs_confirmation and current_item.requested_to_delete):
			inventory.erase(current_item.item)
			item_deleted.emit(current_item.item)
			
			for index in inventory.size():
				var angle = deg_to_rad(rad_to_deg(TAU) / inventory.size() * index + 1)
				var x = radius * cos(angle)
				var z = radius * sin(angle)
				var item = inventory[index]
				
				var tween = create_tween()
				tween.tween_property(item, "position", Vector3(x, 0, z), next_item_spin_duration)
				
			current_item.item.queue_free()
			current_item.item = null if inventory.is_empty() else inventory.back()
			current_item.requested_to_delete = false
			
	
