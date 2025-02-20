extends Node

### PLAYER ###
@warning_ignore("unused_signal")
signal lock_player
@warning_ignore("unused_signal")
signal unlock_player


### CAMERA ###
@warning_ignore("unused_signal")
signal transition_camera_2d_requested(from: Camera2D, to: Camera2D, duration: float)
@warning_ignore("unused_signal")
signal transition_camera_3d_requested(from: Camera3D, to: Camera3D, duration: float)


### INTERACTIONS ###
@warning_ignore("unused_signal")
signal interacted(interactor)
@warning_ignore("unused_signal")
signal canceled_interaction(interactor)
@warning_ignore("unused_signal")
signal focused(interactor)
@warning_ignore("unused_signal")
signal unfocused(interactor)


### INPUT ###
@warning_ignore("unused_signal")
signal controller_connected(device: int, controller_name: String)
@warning_ignore("unused_signal")
signal controller_disconnected(device: int, controller_name: String)
