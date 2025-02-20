class_name TextDisplay extends RichTextLabel

signal finished_display

const BBCODE_START_FLAG: StringName = "["
const BBCODE_END_FLAG: StringName = "]"

@export var content: String
@export var manual_start := false
@export var input_action_to_start := "ui_accept"
@export var can_be_skipped := true
@export var input_action_to_skip := "ui_accept"
@export var letter_time := 0.03
@export var space_time := 0.06
@export var punctuation_time := 0.2

var letter_index = 0
var typing_timer: Timer

var current_bbcode := ""
var bbcode_flag = false

var is_typing := false
var typing_finished := false


func _init(content_to_display: String = content):
	content = content_to_display


func _unhandled_input(_event: InputEvent):
	if(not typing_finished):
		
		if not is_typing and manual_start and InputWizard.action_just_pressed_and_exists(input_action_to_start):
			display_letters()
		
		elif is_typing and can_be_skipped and InputWizard.action_just_pressed_and_exists(input_action_to_skip):
			skip()
			

func _ready():
	if(content.is_empty()):
		push_warning("The TypedText node %s needs a content to display, node is removed" % name)
		queue_free()
		return
	
	bbcode_enabled = true
	fit_content = true
	
	_create_typing_timer()
	finished_display.connect(on_finished_display)

	if(not manual_start):
		display_letters()

				
func display_letters():
	if(typing_finished):
		return
	
	if letter_index >= content.length():
		finished_display.emit()
		
		if is_instance_valid(typing_timer):
			typing_timer.stop()
		return
	
	is_typing = true
	
	var next_character: String = content[letter_index]
		
	letter_index += 1
	
	if !bbcode_flag && next_character == BBCODE_START_FLAG:
		bbcode_flag = true
	
	if bbcode_flag:
		current_bbcode += next_character
		bbcode_flag = next_character != BBCODE_END_FLAG
		display_letters()
		return
	else:
		if not current_bbcode.is_empty():
			text += current_bbcode + next_character
			current_bbcode = ""
			display_letters()
			return
			
	text += next_character
	
	match[content[letter_index]]:
		".", ",", ":", ";", "!", "¡", "?", "¿":
			typing_timer.start(punctuation_time)
		" ":
			typing_timer.start(space_time)
		_:
			typing_timer.start(letter_time)


func skip():
	if is_typing:
		text = ""
		append_text(content)
		finished_display.emit()


func _create_typing_timer():
	if typing_timer:
		return
		
	typing_timer = Timer.new()
	typing_timer.name = "TypingTimer"
	typing_timer.process_callback = Timer.TIMER_PROCESS_IDLE
	typing_timer.one_shot = true
	typing_timer.autostart = false
	
	add_child(typing_timer)
	typing_timer.timeout.connect(on_typing_timer_timeout)


func on_typing_timer_timeout():
	display_letters()


func on_finished_display():
	letter_index = 0
	content = ""
	is_typing = false
	typing_finished = true
	set_process_unhandled_input(false)
	
	if is_instance_valid(typing_timer):
		typing_timer.stop()
