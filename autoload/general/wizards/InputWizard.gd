class_name InputWizard

static var numeric_keys = [48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 96, 97, 98, 99, 100, 101, 102, 103, 104, 105]


static func is_mouse_left_click(event: InputEvent) -> bool:
	if(event is InputEventMouseButton):
		return event.button_index == MOUSE_BUTTON_LEFT && event.pressed
		
	return false
	

static func is_mouse_right_click(event: InputEvent) -> bool:
	if(event is InputEventMouseButton):
		return event.button_index == MOUSE_BUTTON_RIGHT && event.pressed
		
	return false
	
	
static func numeric_key_pressed(event: InputEvent) -> bool:
	if event is InputEventKey:
		return event.pressed && (numeric_keys.has(int(event.keycode)) || numeric_keys.has(int(event.physical_keycode)) )
		
	return false


static func readable_key(key: InputEventKey):
	var key_with_modifiers: Key = key.get_physical_keycode_with_modifiers() if key.keycode == KEY_NONE else key.get_keycode_with_modifiers()
	
	return OS.get_keycode_string(key_with_modifiers).replace("+", "+ ")


static func is_gamepad_input(event: InputEvent) -> bool:
	return event is InputEventJoypadButton or event is InputEventJoypadMotion


static func action_just_pressed_and_exists(action: String) -> bool:
	return InputMap.has_action(action) and Input.is_action_just_pressed(action)


static func action_pressed_and_exists(event: InputEvent, action: String) -> bool:
	return InputMap.has_action(action) and event.is_action_pressed(action)


static func action_just_released_and_exists(action: String) -> bool:
	return InputMap.has_action(action) and Input.is_action_just_released(action)


static func action_released_and_exists(event: InputEvent, action: String) -> bool:
	return InputMap.has_action(action) and event.is_action_released(action)


static func is_any_action_just_pressed(_event:InputEvent, actions: Array = []):
	for action in actions:
		if Input.is_action_just_pressed(action):
			return true
			
	return false
	

static func is_any_action_pressed(event: InputEvent, actions: Array = []):
	for action in actions:
		if event.is_action_pressed(action):
			return true
			
	return false


static func is_any_action_released(event:InputEvent, actions: Array = []):
	for action in actions:
		if event.is_action_released(action):
			return true
			
	return false
