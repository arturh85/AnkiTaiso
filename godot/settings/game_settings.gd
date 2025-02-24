class_name GameSettings extends Resource

## Remember to set the volume sliders with max value of 1 and step of 0.001 to apply the volume changes properly.
@export var AUDIO_VOLUMES := {
	"music": 1.0,
	"sfx": 1.0,
	"voice": 1.0,
	"ui": 1.0,
	"ambient": 1.0
}

@export_range(0.1, 20.0, 0.1) var MOUSE_SENSITIVITY := 3.0
@export var DISPLAY_MODE := DisplayServer.WINDOW_MODE_WINDOWED
@export var VSYNC := DisplayServer.VSYNC_DISABLED
@export var ANTIALIASING :=  Viewport.MSAA_DISABLED
@export var RESOLUTIONS := [
	{"value":  Vector2(320, 180), "enabled": true},
	{"value":  Vector2(480, 270), "enabled": true},
	{"value":  Vector2(640, 360), "enabled": true},
	{"value":  Vector2(640, 480), "enabled": true},
	{"value":  Vector2(960, 540), "enabled": true},
	{"value":  Vector2(1280, 720), "enabled": true},
	{"value":  Vector2(1440, 810), "enabled": true},
	{"value":  Vector2(1920, 1080), "enabled": true},
 ]

@export var RESOLUTIONS_4_TO_3 := [
		{"value": Vector2(512, 384),  "enabled": true}, 
		{"value": Vector2(768, 576),  "enabled": true}, 
		{"value": Vector2(1024, 768),  "enabled": true}, 
]

var resolutions: = {
	"16to9":[
		Vector2(320, 180), 
		Vector2(400, 224), 
		Vector2(640, 360), 
		Vector2(960, 540), 
		Vector2(1366, 768), 
		Vector2(1600, 900), 
		Vector2(1920, 1080), 
		Vector2(2560, 1440), 
		Vector2(3840, 2160), 
	], 
	"4to3":[
		Vector2(512, 384), 
		Vector2(768, 576), 
		Vector2(1024, 768), 
	], 
	"16to10":[
		Vector2(960, 600), 
		Vector2(1280, 800), 
		Vector2(1680, 1050), 
		Vector2(1920, 1200), 
		Vector2(2560, 1600), 
	], 
	"21to9":[
		Vector2(1280, 540), 
		Vector2(1720, 720), 
		Vector2(2560, 1080), 
		Vector2(3440, 1440), 
	]
}

## https://docs.godotengine.org/en/stable/tutorials/i18n/locales.html
@export var language := "english"
@export var languages = {
	"english":"en", 
	"french":"fr", 
	"schinese":"zh", 
	"tchinese":"zh_TW", 
	"japanese":"ja", 
	"koreana":"ko", 
	"russian":"ru", 
	"polish":"pl", 
	"spanish":"es", 
	"brazilian":"pt", 
	"german":"de", 
	"turkish":"tr", 
	"italian":"it"
}

## This only set the volume value into the variable not the bus.
func set_volume(bus: String, value: float):
	bus = bus.to_lower()
	
	if AUDIO_VOLUMES.has(bus):
		AUDIO_VOLUMES[bus] = value
