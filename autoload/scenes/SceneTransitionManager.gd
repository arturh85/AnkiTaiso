extends Node

@export var LOADING_SCREEN_SCENE: PackedScene = preload("res://components/ui/loading/LoadingScreen.tscn")

@onready var transition_animation_player: AnimationPlayer = $AnimationPlayer

enum TRANSITIONS {
	NO_TRANSITION,
	FADE_TO_BLACK,
	FADE_FROM_BLACK
}

var next_scene_path: String
var remaining_animations := []


func _ready():
	transition_animation_player.animation_finished.connect(on_animation_finished)
	

func reload_current_scene() -> void:
	get_tree().reload_current_scene()


func transition_to_scene(
	scene, 
	loading_screen: bool = false,
	out_transition: TRANSITIONS = TRANSITIONS.FADE_TO_BLACK,
	in_transition: TRANSITIONS =  TRANSITIONS.FADE_FROM_BLACK, 
):
	_prepare_transition_animations(loading_screen, out_transition, in_transition)
	next_scene_path = scene.resource_path if scene is PackedScene else scene
	
	if typeof(scene) == TYPE_STRING and _is_valid_scene_path(scene):
		if not remaining_animations.is_empty():
			await trigger_transition(remaining_animations.pop_back())	
			
		_transition_to_scene_file(scene, loading_screen)
	
	if scene is PackedScene:
		if not remaining_animations.is_empty():
			await trigger_transition(remaining_animations.pop_back())
				
		_transition_to_packed_scene(scene, loading_screen)


func _prepare_transition_animations(loading_screen: bool, out_transition: TRANSITIONS, in_transition: TRANSITIONS):
	remaining_animations.clear()
	
	if loading_screen:
		remaining_animations.append(out_transition)
	else:
		remaining_animations.append_array([in_transition, out_transition])
		
	remaining_animations = remaining_animations.filter(func(transition): return transition != TRANSITIONS.NO_TRANSITION)

	
func _transition_to_scene_file(scene_path: String, loading_screen: bool = false):
	if loading_screen:
		_transition_with_loading_screen(scene_path)
	else:
		get_tree().call_deferred("change_scene_to_file", scene_path)
	

func _transition_to_packed_scene(scene: PackedScene,  loading_screen: bool = false):
	if loading_screen:
		_transition_with_loading_screen(scene.resource_path)
	else:	
		get_tree().call_deferred("change_scene_to_packed", scene)
	
	
func _transition_with_loading_screen(_scene):
	if LOADING_SCREEN_SCENE:
		get_tree().call_deferred("change_scene_to_packed", LOADING_SCREEN_SCENE)
		
	
func trigger_transition(transition: TRANSITIONS) -> void:
	if transition_animation_player.is_playing():
		return
		
	var transition_name: String = _enum_transition_to_animation_name(transition)
	
	if transition_animation_player.get_animation_list().has(transition_name):
		transition_animation_player.play(transition_name)
		await transition_animation_player.animation_finished


func _enum_transition_to_animation_name(transition: TRANSITIONS) -> String:
	var transition_name: String = ""
	
	match transition:
		TRANSITIONS.FADE_TO_BLACK:
			transition_name = "fade_to_black"
		TRANSITIONS.FADE_FROM_BLACK:
			transition_name = "fade_from_black"
		_:
			transition_name = ""
			
	return transition_name


func _is_valid_scene_path(scene: String) -> bool:
	return not scene.is_empty() and scene.is_absolute_path() and ResourceLoader.exists(scene)


func on_animation_finished(_animation_name: String):
	if remaining_animations.is_empty():
		return
		
	var animation = _enum_transition_to_animation_name(remaining_animations.pop_back())
	
	if animation and transition_animation_player.get_animation_list().has(animation):
		transition_animation_player.play(animation)

