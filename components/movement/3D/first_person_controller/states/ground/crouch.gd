class_name FirstPersonControllerCrouch extends FirstPersonControllerGroundState

@export var speed := 3.0

@onready var animation_player: AnimationPlayer = actor.get_node("AnimationPlayer")


func _enter():
	var last_state: MachineState = FSM.last_state()
	var last_state_was_crawl_or_slide = last_state and ["Crawl", "Slide"].any(func(state: String): return last_state.name.to_lower() == state.to_lower())
	
	if not last_state_was_crawl_or_slide:
		animation_player.play("crouch")


func _exit(next_state):
	if not next_state is FirstPersonControllerCrawl:
		animation_player.play_backwards("crouch")
		await animation_player.animation_finished
		actor.stand_collision_shape.disabled = false
	
	
func physics_update(delta):
	super.physics_update(delta)
	
	if not Input.is_action_pressed("crouch") and not actor.ceil_shape_cast.is_colliding():
		if actor.velocity.is_zero_approx():
			FSM.change_state_to("Idle")
		else:
			FSM.change_state_to("Walk")
	
	move(speed)
	
	stair_stepper.stair_step_up()

	actor.move_and_slide()
	
	stair_stepper.stair_step_down()
	
	detect_jump()
	detect_crawl()
	
	
