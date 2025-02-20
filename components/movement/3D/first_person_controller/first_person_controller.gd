@icon("res://components/movement/3D/first_person_controller/first_person_controller.svg")
class_name FirstPersonController extends CharacterBody3D

signal lock_requested
signal unlock_requested

@export_group("Motion Mechanics")
@export var run := true
@export var slide := false
@export var jump := true
@export var wall_jump := false
@export var wall_run := false
@export var crouch := true
@export var crawl := false
@export var stair_stepper := true

@export_group("Effects")
@export var swing_head:= false
@export var head_bobbing:= false

@onready var stand_collision_shape: CollisionShape3D = $StandCollisionShape
@onready var crouch_collision_shape: CollisionShape3D = $CrouchCollisionShape
@onready var crawl_collision_shape: CollisionShape3D = $CrawlCollisionShape
@onready var ceil_shape_cast: ShapeCast3D = %CeilShapeCast

@onready var stair_stepper_3d = $StairStepper3D as StairStepper3D
@onready var wall_detector_3d = $WallDetector3D as WallDetector3D
@onready var finite_state_machine := $MotionFiniteStateMachine as FiniteStateMachine
@onready var shake_camera_3d  := %ShakeCamera3D as ShakeCamera3D
@onready var first_person_camera_mouse_rotator := $FirstPersonCameraMouseRotator as FirstPersonCameraMouseRotator
@onready var swing_head_effect := $SwingHeadEffect as SwingHeadEffect
@onready var head_bobbing_effect := $HeadBobbingEffect as HeadBobbingEffect

@onready var transformed_input := TransformedInputDirection.new(self)
@onready var is_on_ground = finite_state_machine.current_state is FirstPersonControllerGroundState

var locked := false:
	set(value):
		if value != locked:
			if value:
				lock_requested.emit()
			else:
				unlock_requested.emit()
		locked = value


func _unhandled_key_input(_event: InputEvent):
	if Input.is_action_just_pressed("ui_cancel"):
		switch_mouse_mode()


func _ready():
	Input.mouse_mode = Input.MOUSE_MODE_CAPTURED

	stair_stepper_3d.stair_stepping_enabled = stair_stepper
	
	stand_collision_shape.disabled = false
	crouch_collision_shape.disabled = true
	crawl_collision_shape.disabled = true
	
	lock_requested.connect(on_lock_requested)
	unlock_requested.connect(on_unlock_requested)
	finite_state_machine.state_changed.connect(on_state_changed)
	
	_register_fsm_transitions()


func _physics_process(delta):
	transformed_input.update_input_direction()
#
	if swing_head and is_on_ground:
		swing_head_effect.apply(transformed_input.input_direction)
		
	if head_bobbing and is_on_ground:
		head_bobbing_effect.apply(velocity, delta)


func switch_mouse_mode():
	if Input.mouse_mode == Input.MOUSE_MODE_CAPTURED:
		Input.mouse_mode = Input.MOUSE_MODE_VISIBLE
	else:
		Input.mouse_mode = Input.MOUSE_MODE_CAPTURED


func _register_fsm_transitions():
	finite_state_machine.register_transition("WalkToRunTransition", WalkToRunTransition.new())
	finite_state_machine.register_transition("RunToWalkTransition",  RunToWalkTransition.new())
	finite_state_machine.register_transition("IdleToJumpTransition", AnyToJumpTransition.new())	
	finite_state_machine.register_transition("WalkToJumpTransition", AnyToJumpTransition.new())	
	finite_state_machine.register_transition("RunToJumpTransition", AnyToJumpTransition.new())	
	finite_state_machine.register_transition("WallRunToJumpTransition", WallRunToJumpTransition.new())


func _update_collisions_based_on_state(state: MachineState):
	match state.name.to_lower():
		"idle", "walk", "run":
			stand_collision_shape.disabled = false
			crouch_collision_shape.disabled = true
			crawl_collision_shape.disabled = true
		"crouch", "slide":
			stand_collision_shape.disabled = true
			crouch_collision_shape.disabled = false
			crawl_collision_shape.disabled = true
		"crawl":
			stand_collision_shape.disabled = true
			crouch_collision_shape.disabled = true
			crawl_collision_shape.disabled = false
		_:
			stand_collision_shape.disabled = false
			crouch_collision_shape.disabled = true
			crawl_collision_shape.disabled = true


func on_lock_requested():
	first_person_camera_mouse_rotator.lock()
	
	
func on_unlock_requested():
	first_person_camera_mouse_rotator.unlock()	


func on_state_changed(_from: MachineState, to: MachineState):
	is_on_ground = to is FirstPersonControllerGroundState
	
	_update_collisions_based_on_state(to)
	stair_stepper_3d.stair_stepping_enabled = stair_stepper and to is FirstPersonControllerGroundState
	
	if wall_detector_3d.wall_detection_timer.is_stopped() and to is FirstPersonControllerAirState:
		wall_detector_3d.enable_detection()
	else:
		wall_detector_3d.disable_detection()

