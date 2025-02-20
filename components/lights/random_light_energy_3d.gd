@icon("res://components/lights/random_light_energy.svg")
class_name RandomLightEnergy3D extends Node

@export var light: Light3D
@export var min_variation: = 0.8
@export var max_variation: = 1.2
@export var speed: = 25.0
@export var autostart := true

@onready var float_time_1 := randf() * TAU
@onready var float_time_2 := randf() * TAU

var original_light_energy: float
var active := false:
	set(value):
		if value != active:
			if value:
				enable()
			else:
				disable()
				
		active = value

func _ready():
	assert(light is Light3D, "RandomLightEnergy3D: This node does not have assigned a Light3D")
	original_light_energy = light.light_energy
	
	active = autostart
	
	if active:
		enable()
	else:
		disable()


func _process(delta):
	float_time_1 = wrapf(float_time_1 + delta * speed, 0, TAU)
	float_time_2 = wrapf(float_time_2 + delta * speed * 1.423, 0, TAU)
	
	var random = (sin(float_time_1) + cos(float_time_2)) / 2
	
	light.light_energy = original_light_energy * remap(random, 0, 1, min_variation, max_variation)


func enable():
	set_process(true)


func disable():
	set_process(false)
