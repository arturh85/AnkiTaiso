@icon("res://components/audio/foostep_manager/footstep_manager_3d.svg")
class_name FootstepManager3D extends Node3D

@export var floor_detector_raycast: RayCast3D
@export var default_interval_time := 0.6

var sounds_material := {}:
	set(value):
		if not value.is_empty():
			_prepare_sounds(value)
		
		sounds_material = value
		
var interval_timer: Timer
var sfx_playing := false


func _ready():
	assert(floor_detector_raycast is RayCast3D, "FoostepManager: This node needs a raycast to detect the ground material")
	
	if sounds_material.is_empty():
		push_error("FootstepManager: The sounds material dictionary does not have any key/value to map the material group to a sound")

	_create_interval_timer()
	_prepare_sounds(sounds_material)
	
	
func foostep(interval: float = default_interval_time):
	if not sounds_material.is_empty() and not sfx_playing and is_instance_valid(interval_timer) and interval_timer.is_stopped() and floor_detector_raycast.is_colliding():
		var collider = floor_detector_raycast.get_collider()
		
		if collider is StaticBody3D or collider is Area3D:
			var groups = collider.get_groups()
			
			if groups.size() > 0:
				if sounds_material.has(groups[0]):
					var audio_stream_player = sounds_material[groups[0]]
					
					if audio_stream_player is SoundQueue:
						audio_stream_player.play_sound_with_pitch_range()
						interval_timer.start(interval)
						sfx_playing = true
						
					if audio_stream_player is AudioStreamPlayer or audio_stream_player is AudioStreamPlayer3D:
						audio_stream_player.play()
						interval_timer.start(interval)
						sfx_playing = true
	

func _prepare_sounds(data: Dictionary = sounds_material):
	for group in data.keys():
		if data[group] is AudioStream:
			var sound_queue = SoundQueue.new()
			var audio_stream_player = AudioStreamPlayer3D.new()
			audio_stream_player.stream = data[group]
			sound_queue.add_child(audio_stream_player)
			add_child(sound_queue)
			data[group] = sound_queue


func _create_interval_timer():
	if interval_timer == null:
		interval_timer = Timer.new()
		interval_timer.name = "FoostepIntervalTimer"
		interval_timer.process_callback = Timer.TIMER_PROCESS_PHYSICS
		interval_timer.autostart = false
		interval_timer.one_shot = true
		
		add_child(interval_timer)
		interval_timer.timeout.connect(on_interval_timer_timeout)


func on_interval_timer_timeout():
	sfx_playing = false
