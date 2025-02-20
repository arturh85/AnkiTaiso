class_name FirstPersonControllerCrawl extends FirstPersonControllerGroundState

@export var speed := 1.5

@onready var animation_player: AnimationPlayer = actor.get_node("AnimationPlayer")


func _enter():
	animation_player.play("crawl")
	

func _exit(_next_state):
	animation_player.play_backwards("crawl")


func physics_update(delta):
	super.physics_update(delta)
	
	if not Input.is_action_pressed(crouch_input_action) and not actor.ceil_shape_cast.is_colliding():
		FSM.change_state_to("Crouch")
	
	
	move(speed)	
	actor.move_and_slide()
	
