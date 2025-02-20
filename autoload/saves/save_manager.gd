extends Node

signal removed_saved_game(filename: String)

## Dictionary<string, SavedGame>
@export var list_of_saved_games := {}
@export var current_saved_game: SavedGame 


func _ready():
	list_of_saved_games = SavedGame.read_user_saved_games()
	
	
func remove(filename: String):
	if list_of_saved_games.is_empty():
		return
		
	if (list_of_saved_games.has(filename)):
		var saved_game: SavedGame = list_of_saved_games[filename] as SavedGame
		saved_game.delete()
		
		list_of_saved_games.erase(filename)
		removed_saved_game.emit(filename)
		return
	
	push_error("Trying to remove a saved game with name %s that does not exists in the list of saved games" % filename)
	
