class_name ConsumableAudioStreamPlayer2D extends AudioStreamPlayer2D

## The amount of times this audio can be played before being removed from tree
@export var number_of_reproductions := 1:
	set(value):
		number_of_reproductions = max(0, value)


func _init(reproductions: int = number_of_reproductions, _stream: AudioStream = null):
	number_of_reproductions = reproductions
	stream = _stream
	

func _ready():
	if stream == null:
		queue_free()
		
	finished.connect(on_finished_audio)
	

func on_finished_audio():
	number_of_reproductions -= 1
	
	if number_of_reproductions == 0:
		queue_free()
