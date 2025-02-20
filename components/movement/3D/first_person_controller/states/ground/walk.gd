class_name FirstPersonControllerWalk extends FirstPersonControllerGroundState

@export var speed := 5.0
@export var acceleration := 25 
@export var friction := 0
@export var catching_breath_recovery_time := 3.0

var catching_breath_timer: Timer


func _ready():
	_create_catching_breath_timer()
	

func _enter():
	actor.velocity.y = 0


func physics_update(delta):
	super.physics_update(delta)
	
	if actor.transformed_input.input_direction.is_zero_approx():
		FSM.change_state_to("Idle")
		
	if InputMap.has_action(run_input_action) and Input.is_action_pressed(run_input_action):
		FSM.change_state_to("Run")
		
	move(speed, acceleration, friction)
	
		
	stair_stepper.stair_step_up()

	actor.move_and_slide()
	
	stair_stepper.stair_step_down()
	
	detect_jump()
	detect_crouch()


func _create_catching_breath_timer():
	if catching_breath_timer == null:
		catching_breath_timer = Timer.new()
		catching_breath_timer.name = "CatchingBreathTimer"
		catching_breath_timer.process_callback = Timer.TIMER_PROCESS_PHYSICS
		catching_breath_timer.wait_time = catching_breath_recovery_time
		catching_breath_timer.autostart = false
		catching_breath_timer.one_shot = true
		
		add_child(catching_breath_timer)
	
