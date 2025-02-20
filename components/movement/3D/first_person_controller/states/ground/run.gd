class_name FirstPersonControllerRun extends FirstPersonControllerGroundState

@export var speed := 8.0
@export var acceleration := 25 
@export var friction := 0
@export var sprint_time := 4.0

var sprint_timer: Timer
var in_recovery := false


func _ready():
	_create_sprint_timer()


func _enter():
	if sprint_time > 0 and is_instance_valid(sprint_timer):
		sprint_timer.start()
		
	in_recovery = false
	actor.velocity.y = 0
	
	
func _exit(_next_state):
	if is_instance_valid(sprint_timer):
		sprint_timer.stop()
	
	
func physics_update(delta):
	super.physics_update(delta)
	
	if Input.is_action_just_released(run_input_action):
		if actor.velocity.is_zero_approx():
			FSM.change_state_to("Idle")
		else:
			FSM.change_state_to("Walk")
		
	move(speed, acceleration, friction)
	
	if actor.slide and InputMap.has_action(crouch_input_action) and Input.is_action_just_pressed(crouch_input_action):
		FSM.change_state_to("Slide")
	
	stair_stepper.stair_step_up()

	actor.move_and_slide()
	
	stair_stepper.stair_step_down()
	
	detect_jump()


func _create_sprint_timer():
	if sprint_timer == null:
		sprint_timer = Timer.new()
		sprint_timer.name = "SprintTimer"
		sprint_timer.wait_time = sprint_time
		sprint_timer.process_callback = Timer.TIMER_PROCESS_PHYSICS
		sprint_timer.autostart = false
		sprint_timer.one_shot = true
		
		add_child(sprint_timer)
		sprint_timer.timeout.connect(on_sprint_timer_timeout)


func on_sprint_timer_timeout():
	in_recovery = true
	FSM.change_state_to("Walk")
