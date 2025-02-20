class_name InteractionPointer extends Control


@export var minimum_size := Vector2(64, 64)
@export var default_pointer_texture: Texture2D

@onready var current_pointer: TextureRect = %Pointer


func _ready():
	mouse_filter = Control.MOUSE_FILTER_PASS
	
	current_pointer.texture = default_pointer_texture
	current_pointer.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	current_pointer.custom_minimum_size = minimum_size
