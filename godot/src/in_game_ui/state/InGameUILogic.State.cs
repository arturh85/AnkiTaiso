namespace ankitaiso.in_game_ui.state;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using game.domain;
using game_typing.domain;

public partial class InGameUILogic {
  [Meta]
  public partial record State : StateLogic<State> {
    public State() {
      OnAttach(() => {
        var gameRepo = Get<IGameTypingRepo>();
        gameRepo.ClearedWords.Sync += OnWordCleared;
        gameRepo.TotalWords.Sync += OnTotalChanged;
      });

      OnDetach(() => {
        var gameRepo = Get<IGameTypingRepo>();
        gameRepo.ClearedWords.Sync -= OnWordCleared;
        gameRepo.TotalWords.Sync -= OnTotalChanged;
      });
    }

    public void OnWordCleared(int cleared) =>
      Output(new Output.NumCoinsChanged(cleared, Get<IGameTypingRepo>().TotalWords.Value));

    public void OnTotalChanged(int total) =>
      Output(new Output.NumCoinsChanged(Get<IGameTypingRepo>().ClearedWords.Value, total));
  }
}
