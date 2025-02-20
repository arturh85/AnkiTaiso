extends Control

@export var actor: CharacterBody3D
@onready var fps_info: Label = %FPS
@onready var fsm_info: Label = %FSM
@onready var velocity_info: Label = %Velocity
@onready var vsync_info: Label = %Vsync
@onready var memory_info: Label = %Memory

var fsm: FiniteStateMachine

func _ready():
	assert(actor is CharacterBody3D, "DebugInformation: This hud needs a valid first person controller to display the information")
	
	mouse_filter = Control.MOUSE_FILTER_PASS
	
	fsm = actor.get_node("MotionFiniteStateMachine")
	fsm.state_changed.connect(on_state_changed)
	
	fsm_info.text = "State: %s --> %s" % [fsm.current_state.name, ""]
	vsync_info.text = "VSync: " + ("Enabled" if DisplayServer.window_get_vsync_mode() else "Disabled")
	

func _physics_process(_delta):
	fps_info.text = "FPS: %s" % Engine.get_frames_per_second()
	velocity_info.text = "Velocity %s " % actor.velocity
	memory_info.text = "Memory: " + "%3.2f" % (OS.get_static_memory_usage() / 1048576.0) + " MiB"


func on_state_changed(from: MachineState, to: MachineState):
	fsm_info.text = "State: %s --> %s" % [from.name, to.name]

### MEMORY DISPLAY EXPLANATION ###

#%3.2f: This is a format specifier used for floating-point numbers. It ensures the displayed value has:
#
#%3: Total width of at least 3 characters (pads with spaces if needed).
#.2: Exactly 2 digits after the decimal point.
#OS.get_static_memory_usage(): This function retrieves the amount of static memory used by the Godot application.
#
#/ 1048576.0: This divides the memory usage by 1048576.0 (which is 2 raised to the power of 20). This converts bytes (the unit returned by OS.get_static_memory_usage()) to Megabytes (MiB).
#
#+ " MiB": This adds the unit "MiB" (Megabytes) to the formatted memory value.
