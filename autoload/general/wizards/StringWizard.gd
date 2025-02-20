class_name StringWizard

static var random_number_generator := RandomNumberGenerator.new()

static func generate_random_string(length: int = 25, characters: String = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789") -> String:
	var result := ""
	
	if(not characters.is_empty() && length > 0):
		for character in range(length):
			result += characters[random_number_generator.randi() % characters.length()]
			
	return result
	
## Converts PascalCaseString into pascal_case_string
static func camel_to_snake(camel_string: String) -> String:
	var snake_string := ""
	var previous_char := ""
	
	for c in camel_string:
		if c.to_upper() == c and previous_char != "" and previous_char.to_upper() != previous_char:
			snake_string += "_"
		snake_string += c.to_lower()
		previous_char = c
	
	return snake_string

## Converts pascal_case_string into PascalCaseString
static func snake_to_camel_case(screaming_snake_case: String) -> String:
	var words := screaming_snake_case.split("_")
	var camel_case := ""
	
	for i in range(words.size()):
		camel_case += words[i].capitalize()
	
	return camel_case
