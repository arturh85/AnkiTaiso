@icon("res://components/interaction/interactable_interactor/interactable.svg")
class_name Interactable3D extends Area3D

signal interacted(interactor)
signal canceled_interaction(interactor)
signal focused(interactor)
signal unfocused(interactor)

enum CATEGORY {
	DOOR,
	WINDOW,
	OBJECT,
	ITEM,
	WEAPON,
	KEY
}

@export_group("Information")
@export var title: String
@export var description: String
@export var category: CATEGORY

@export_group("Scan")
@export var scannable := false

@export_group("Pickup")
@export var pickable := false
@export var pickup_message: String
@export var pull_power := 20.0
@export var throw_power := 10.0

@export_group("Usable")
@export var usable := false
@export var usable_message: String

@export_group("Inventory")
@export var can_be_saved := false
@export var inventory_save_message := "Press [I] to save in the inventory"

@export_group("Player")
@export var lock_player := false

@export var focus_pointer: Texture2D
@export var interact_pointer: Texture2D


func _ready():
	priority = 3
	collision_layer = 8 ## interactables
	collision_mask = 0
	monitorable = true
	monitoring = false
	
	interacted.connect(on_interacted)
	canceled_interaction.connect(on_canceled_interaction)
	focused.connect(on_focused)
	unfocused.connect(on_unfocused)

	
func _is_valid_interactor(interactor) -> bool:
	return interactor is Interactor3D or interactor is MouseRayCastInteractor
	
	
func on_interacted(interactor):
	if _is_valid_interactor(interactor):
		if lock_player:
			GlobalEventBus.lock_player.emit()
			
		GlobalEventBus.interacted.emit(interactor)
		
		
func on_canceled_interaction(interactor):
	if _is_valid_interactor(interactor):
		if lock_player:
			GlobalEventBus.unlock_player.emit()
			
		GlobalEventBus.canceled_interaction.emit(interactor)
		
		
func on_focused(interactor):
	if _is_valid_interactor(interactor):
		GlobalEventBus.focused.emit(interactor)


func on_unfocused(interactor):
	if _is_valid_interactor(interactor):
		GlobalEventBus.unfocused.emit(interactor)
