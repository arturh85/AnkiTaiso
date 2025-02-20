extends Node

signal achievement_unlocked(_name: String, achievement: Dictionary)
signal achievement_updated(_name: String, achievement: Dictionary)
signal achievement_reset(_name: String, achievement: Dictionary)
signal remote_achievements_loaded
signal all_achievements_unlocked

@export var local_source_path := "res://"
@export var remote_source_url := "https://"
@export var filename := "achievements"
@export var password := "Yjtcz9mfLk7nDABrQYI9y0fyX"

@onready var http_request: HTTPRequest = $HTTPRequest

var current_achievements: Dictionary = {}
var unlocked_achievements: Dictionary = {}
var achievements_keys: PackedStringArray = []

## Url to test reading remote json files https://mysafeinfo.com/api/data?list=englishmonarchs&format=json


func _ready():
	http_request.request_completed.connect(_on_request_completed)
	achievement_updated.connect(_on_achievement_updated)
	achievement_unlocked.connect(_on_achievement_updated)
	
	_create_save_directory(_save_directory_path())
	_prepare_achievements()


func _save_directory_path() -> String:
	return OS.get_user_data_dir() + "/achievements"
	
	
func get_achievement(_name: String) -> Dictionary:
	if current_achievements.has(_name):
		return current_achievements[_name]
	
	return {}

func update_achievement(_name: String, data: Dictionary) -> void:
	if current_achievements.has(_name):
		current_achievements[_name].merge(data, true)
		
		achievement_updated.emit(_name, data)
		

func unlock_achievement(_name: String) -> void:
	if current_achievements.has(_name):
		var achievement: Dictionary = current_achievements[_name]
		
		if not achievement["unlocked"]:
			achievement["unlocked"] = true
			unlocked_achievements[_name] = achievement
			achievement_unlocked.emit(_name, achievement)


func reset_achievement(_name: String, data: Dictionary = {}) -> void:
	if current_achievements.has(_name):
		current_achievements[_name].merge(data, true)
		current_achievements[_name]["unlocked"] = false
		current_achievements[_name]["current_progress"] = 0.0
		
		if unlocked_achievements.has(_name):
			unlocked_achievements.erase(_name)
			
		achievement_reset.emit(_name, current_achievements[_name])
		achievement_updated.emit(_name, current_achievements[_name])


func _read_from_local_source() -> void:
	if FileAccess.file_exists(local_source_path):
		var content = JSON.parse_string(FileAccess.get_file_as_string(local_source_path))
		
		if content == null:
			push_error("Achievements: Failed reading achievement file %s" %  local_source_path)
			return
			
		current_achievements = content
		achievements_keys = current_achievements.keys()
	else:
		push_error("The local source file for achievements %s does not exists" % local_source_path)
		

func _read_from_remote_source() -> void:
	if Utilities.is_valid_url(_remote_source_url()):
		http_request.request(_remote_source_url())
		await http_request.request_completed


func _create_save_directory(path: String) -> void:
	if not DirAccess.dir_exists_absolute(path):
		DirAccess.make_dir_absolute(path)


func _prepare_achievements() -> void:
	_read_from_local_source()
	_read_from_remote_source()
	_sync_achievements_with_encrypted_saved_file()
	_update_encrypted_save_file()
	
	for key in current_achievements.keys():
		if current_achievements[key]["unlocked"]:
			unlocked_achievements[key] = current_achievements[key]


func _sync_achievements_with_encrypted_saved_file() -> void:
	var saved_file_path = _encrypted_save_file_path()
	
	if FileAccess.file_exists(saved_file_path):
		var content = FileAccess.open_encrypted_with_pass(saved_file_path, FileAccess.READ, password)
		if content == null:
			push_error("Achievements: Failed reading saved achievement file {path} with error {error}".format({"path": saved_file_path, "error": FileAccess.get_open_error()}))
			return
			
		var achievements = JSON.parse_string(content.get_as_text())
		if achievements:
			current_achievements.merge(achievements, true)


func _check_if_all_achievements_are_unlocked() -> bool:
	var all_unlocked = unlocked_achievements.size() == current_achievements.size()
	
	if all_unlocked:
		all_achievements_unlocked.emit()
		
	return all_unlocked


func _update_encrypted_save_file() -> void:
	if current_achievements.is_empty():
		return
	
	var saved_file_path = _encrypted_save_file_path()

	var file = FileAccess.open_encrypted_with_pass(saved_file_path, FileAccess.WRITE, password)
	if file == null:
		push_error("Achievements: Failed writing saved achievement file {path} with error {error}".format({"path": saved_file_path, "error": FileAccess.get_open_error()}))
		return
	
	file.store_string(JSON.stringify(current_achievements))
	file.close()


func _remote_source_url() -> String:
	return remote_source_url if Utilities.is_valid_url(remote_source_url) else ""


func _encrypted_save_file_path() -> String:
	return "%s/%s.json" % [_save_directory_path(), filename]


func _on_request_completed(result: int, response_code: int, _headers: PackedStringArray, body: PackedByteArray) -> void:
	if result == HTTPRequest.RESULT_SUCCESS:
		var achievements := {}
		var content = JSON.parse_string(body.get_string_from_utf8())

		if typeof(content) == TYPE_ARRAY:			
			for achievement in content:
				achievements[achievement["id"]] = achievement
				current_achievements.merge(achievements, true)
		else:
			current_achievements.merge(content, true)
		
		remote_achievements_loaded.emit()
			
		return
	
	push_error("Achievements: Failed request with code {code} to remote source url from achievements: {body}".format({"body": body, "code": response_code}))


func _on_achievement_updated(_name: String, _achievement: Dictionary) -> void:
	_update_encrypted_save_file()
	_check_if_all_achievements_are_unlocked()
	
	
### BASIC STRUCTURE FOR JSON ACHIEVEMENTS ###	
#{
	#"achievement-name": {
		#"name": "MY achievement",
		#"description": "Kill 25 enemies",
		#"is_secret": false,
		#"count_goal": 25,
		#"current_progress": 0.0,
		#"icon_path": "res://assets/icon/my-achievement.png",
		#"unlocked": false,
		#"active": true
	#}
#}
