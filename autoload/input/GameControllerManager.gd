extends Node

signal controller_connected(device: int, controller_name: String)
signal controller_disconnected(device: int , controller_name: String)

var current_controller_guid
var current_controller_name := DEVICE_KEYBOARD
var connected := false

const DEVICE_GENERIC = "generic"
const DEVICE_KEYBOARD = "keyboard"
const DEVICE_XBOX_CONTROLLER = "xbox"
const DEVICE_SWITCH_CONTROLLER = "switch"
const DEVICE_SWITCH_JOYCON_LEFT_CONTROLLER = "switch_left_joycon"
const DEVICE_SWITCH_JOYCON_RIGHT_CONTROLLER = "switch_right_joycon"
const DEVICE_PLAYSTATION_CONTROLLER = "playstation"
const DEVICE_LUNA_CONTROLLER = "luna"

const XBOX_BUTTON_LABELS = ["A", "B", "X", "Y", "Back", "Home", "Menu", "Left Stick", "Right Stick", "Left Shoulder", "Right Shoulder", "Up", "Down", "Left", "Right", "Share"]
const SWITCH_BUTTON_LABELS = ["B", "A", "Y", "X", "-", "", "+", "Left Stick", "Right Stick", "Left Shoulder", "Right Shoulder", "Up", "Down", "Left", "Right", "Capture"]
const PLAYSTATION_BUTTON_LABELS = ["Cross", "Circle", "Square", "Triangle", "Select", "PS", "Options", "L3", "R3", "L1", "R1", "Up", "Down", "Left", "Right", "Microphone"]


func _ready():
	Input.joy_connection_changed.connect(_on_joy_connection_changed)


func has_joypad() -> bool:
	return Input.get_connected_joypads().size() > 0


func _on_joy_connection_changed(device: int, _connected: bool):
	var controller_name := Input.get_joy_name(device) if _connected else ""
	update_current_controller(device, controller_name)
	
	connected = _connected
	
	if connected:
		controller_connected.emit(device, current_controller_name)
		GlobalEventBus.controller_connected.emit(device, current_controller_name)
	else:
		controller_disconnected.emit(device, current_controller_name)
		GlobalEventBus.controller_disconnected.emit(device, current_controller_name)


func update_current_controller(device: int, controller_name: String) -> void:
	##https://github.com/mdqinc/SDL_GameControllerDB
	current_controller_guid = Input.get_joy_guid(device)
	
	
	match controller_name:
		"Luna Controller":
			current_controller_name = DEVICE_LUNA_CONTROLLER
		"XInput Gamepad", "Xbox Series Controller", "Xbox 360 Controller", \
		"Xbox One Controller": 
			current_controller_name = DEVICE_GENERIC
		"Sony DualSense","Nacon Revolution Unlimited Pro Controller",\
		"PS3 Controller", "PS4 Controller", "PS5 Controller":
			current_controller_name = DEVICE_PLAYSTATION_CONTROLLER
		"Steam Virtual Gamepad": 
			current_controller_name = DEVICE_GENERIC
		"Switch","Switch Controller","Nintendo Switch Pro Controller",\
		"Faceoff Deluxe Wired Pro Controller for Nintendo Switch":
			current_controller_name = DEVICE_SWITCH_CONTROLLER
		"Joy-Con (L)":
			current_controller_name = DEVICE_SWITCH_JOYCON_LEFT_CONTROLLER
		"Joy-Con (R)":
			current_controller_name = DEVICE_SWITCH_JOYCON_RIGHT_CONTROLLER
		_: 
			current_controller_name = DEVICE_KEYBOARD


func current_controller_is_generic() -> bool:
	return current_controller_name == DEVICE_GENERIC


func current_controller_is_luna() -> bool:
	return current_controller_name == DEVICE_LUNA_CONTROLLER


func current_controller_is_keyboard() -> bool:
	return current_controller_name == DEVICE_KEYBOARD


func current_controller_is_playstation() -> bool:
	return current_controller_name == DEVICE_PLAYSTATION_CONTROLLER


func current_controller_is_xbox() -> bool:
	return current_controller_name == DEVICE_XBOX_CONTROLLER


func current_controller_is_switch() -> bool:
	return current_controller_name == DEVICE_SWITCH_CONTROLLER


func current_controller_is_switch_joycon() -> bool:
	return current_controller_name in [DEVICE_SWITCH_JOYCON_LEFT_CONTROLLER, DEVICE_SWITCH_JOYCON_RIGHT_CONTROLLER]


func current_controller_is_switch_joycon_right() -> bool:
	return current_controller_name == DEVICE_SWITCH_JOYCON_RIGHT_CONTROLLER


func current_controller_is_switch_joycon_left() -> bool:
	return current_controller_name == DEVICE_SWITCH_JOYCON_LEFT_CONTROLLER
