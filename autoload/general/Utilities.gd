extends Node

signal frame_freezed_started
signal frame_freezed_finished


func start_frame_freeze(time_scale: float, duration: float) -> void:
	frame_freezed_started.emit()
	
	var original_time_scale_value := Engine.time_scale
	Engine.time_scale = time_scale
	
	await get_tree().create_timer(duration * time_scale).timeout
	Engine.time_scale = original_time_scale_value
	
	frame_freezed_finished.emit()

"""
Formats a time value into a string representation of minutes, seconds, and optionally milliseconds.

Args:
	time (float): The time value to format, in seconds.
	use_milliseconds (bool, optional): Whether to include milliseconds in the formatted string. Defaults to false.

Returns:
	str: A string representation of the formatted time in the format "MM:SS" or "MM:SS:mm", depending on the value of use_milliseconds.

Example:
	# Format 123.456 seconds without milliseconds
	var formatted_time = format_seconds(123.456)
	# Result: "02:03"

	# Format 123.456 seconds with milliseconds
	var formatted_time_with_ms = format_seconds(123.456, true)
	# Result: "02:03:45"
"""
func format_seconds(time: float, use_milliseconds: bool = false) -> String:
	var minutes := floori(time / 60)
	var seconds := fmod(time, 60)
	
	if(not use_milliseconds):
		return "%02d:%02d" % [minutes, seconds]
		
	var milliseconds := fmod(time, 1) * 100
	
	return "%02d:%02d:%02d" % [minutes, seconds, milliseconds]


func is_valid_url(url: String) -> bool:
	var regex = RegEx.new()
	var url_pattern = "/(https:\\/\\/www\\.|http:\\/\\/www\\.|https:\\/\\/|http:\\/\\/)?[a-zA-Z]{2,}(\\.[a-zA-Z]{2,})(\\.[a-zA-Z]{2,})?\\/[a-zA-Z0-9]{2,}|((https:\\/\\/www\\.|http:\\/\\/www\\.|https:\\/\\/|http:\\/\\/)?[a-zA-Z]{2,}(\\.[a-zA-Z]{2,})(\\.[a-zA-Z]{2,})?)|(https:\\/\\/www\\.|http:\\/\\/www\\.|https:\\/\\/|http:\\/\\/)?[a-zA-Z0-9]{2,}\\.[a-zA-Z0-9]{2,}\\.[a-zA-Z0-9]{2,}(\\.[a-zA-Z0-9]{2,})?/g"
	regex.compile(url_pattern)
	
	return regex.search(url) != null
	
	
func open_external_link(url: String) -> void:
	if is_valid_url(url) and OS.has_method("shell_open"):
		if OS.get_name() == "Web":
			url = url.uri_encode()
			
		OS.shell_open(url)


func sigmoid(x: float) -> float:
	return 1 / (1 + exp(-x))

	
func random_enum(_enum):
	return _enum.keys()[randi() % _enum.size()]

@warning_ignore("integer_division")
func integer_to_roman_number(number: int) -> String:
	number = absi(number)
	
	var roman_digits = ["", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX"]
	var tens_place = ["", "X", "XX", "XXX", "XL", "L", "LX", "LXX", "LXXX", "XC"]
	var hundreds_place = ["", "C", "CC", "CCC", "CD", "D", "DC", "DCC", "DCCC", "CM"]
	var thousands_place = ["", "M", "MM", "MMM"]
	
	var thousands := number / 1000
	var hundreds := number % 1000 / 100
	var tens := number % 100 / 10
	var ones := number % 10
	
	var roman_number = "%s%s%s%s" % [thousands_place[thousands], hundreds_place[hundreds], tens_place[tens], roman_digits[ones]]
	
	return roman_number


func roman_number_to_integer(roman_number: String) -> int:
	var roman_to_int_map = {"I": 1, "V": 5, "X": 10, "L": 50, "C": 100, "D": 500, "M": 1000}
	
	var result = 0
	var previous_value = 0
	
	for i in range(roman_number.length() - 1, -1, -1):
		var current_value = roman_to_int_map[roman_number[i]]
		
		if current_value < previous_value:
			result -= current_value
		else:
			result += current_value
		
		previous_value = current_value

	return result


func get_sprite_dimensions(sprite: Sprite2D) -> Vector2:
	var texture: Texture2D = sprite.texture
	var image: Image = texture.get_image()
	var used_rect: Rect2i = image.get_used_rect()
	var sprite_dimensions: Vector2 = Vector2(used_rect.size) * sprite.scale

	return sprite_dimensions


func screenshot(texture_rect: TextureRect) -> TextureRect:
	var img = get_viewport().get_texture().get_image()
	var tex = ImageTexture.create_from_image(img)
	texture_rect.set_texture(tex)
	
	return texture_rect
	

func value_is_between(number: int, min_value: int, max_value: int, inclusive: = true) -> bool:
	if inclusive:
		return number >= min(min_value, max_value) and number <= max(min_value, max_value)
	else :
		return number > min(min_value, max_value) and number < max(min_value, max_value)


func decimal_value_is_between(number: float, min_value: float, max_value: float, inclusive: = true, precision: float = 0.00001) -> bool:
	if inclusive:
		min_value -= precision
		max_value += precision

	return number >= min(min_value, max_value) and number <= max(min_value, max_value)



func add_thousand_separator(number) -> String:
	var number_as_text = str(number)
	var mod = number_as_text.length() % 3
	var result := ""
	
	for index in range(0, number_as_text.length()):
		if index != 0 and index % 3 == mod:
			result += ","
			
		result += number_as_text[index]
		
	return result


func reverse_key_value_in_dictionary(source_dict: Dictionary) -> Dictionary:
	var reversed := {}
	
	for key in source_dict.keys():
		reversed[source_dict[key]] = key
	
	return reversed
	
