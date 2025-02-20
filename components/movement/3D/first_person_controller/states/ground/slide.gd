class_name FirstPersonControllerSlide extends FirstPersonControllerGroundState

@export var speed := 3.0
@export var slide_time := 0.9
@export var slide_tilt = 7.0
@export var friction_momentum := 0.1
@export var reduce_speed_gradually := true
@export var swing_head := true

@onready var eyes: Node3D = actor.get_node("%Eyes")
@onready var animation_player: AnimationPlayer = actor.get_node("AnimationPlayer")

var slide_timer: Timer
var last_direction: Vector3 = Vector3.ZERO
var decrease_rate: float
var slide_side: int
var rng = RandomNumberGenerator.new()


func _ready():
	_create_slide_timer()


func _enter():
	if is_instance_valid(slide_timer):
		slide_timer.start()
	
	last_direction = actor.transformed_input.world_coordinate_space_direction
	decrease_rate = slide_time
	animation_player.play("crouch")
	slide_side = sign(rng.randi_range(-1, 1))
	

func _exit(next_state):
	if is_instance_valid(slide_timer):
		slide_timer.stop()
	
	decrease_rate = slide_time
	
	if eyes.rotation.z != 0:
		var tween = create_tween()
		tween.tween_property(eyes, "rotation:z", 0, 0.35).set_ease(Tween.EASE_OUT)
		
	if not next_state is FirstPersonControllerCrouch:
		animation_player.play_backwards("crouch")
		await animation_player.animation_finished
		actor.stand_collision_shape.disabled = false
	

func physics_update(delta):
	super.physics_update(delta)
	
	if reduce_speed_gradually:
		decrease_rate -= delta
		
	var momentum = decrease_rate + friction_momentum
	
	actor.velocity = Vector3(last_direction.x * momentum * speed, actor.velocity.y, last_direction.z * momentum * speed)
	
	if swing_head and slide_tilt > 0:
		eyes.rotation.z = lerp(eyes.rotation.z, slide_side * deg_to_rad(slide_tilt), delta * 8.0)
		
	if not actor.ceil_shape_cast.is_colliding():
		detect_jump()
		
	actor.move_and_slide()


func _create_slide_timer():
	if slide_timer == null:
		slide_timer = Timer.new()
		slide_timer.name = "SlideTimer"
		slide_timer.wait_time = slide_time
		slide_timer.process_callback = Timer.TIMER_PROCESS_PHYSICS
		slide_timer.autostart = false
		slide_timer.one_shot = true
		
		add_child(slide_timer)
		slide_timer.timeout.connect(on_slide_timer_timeout)


func on_slide_timer_timeout():
	detect_crouch()
	
	if actor.ceil_shape_cast.is_colliding():
		FSM.change_state_to("Crouch")
	else:
		FSM.change_state_to("Walk")
