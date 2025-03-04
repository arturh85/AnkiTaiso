using Godot;
using System;
using Enemy = kyoukaitansa.enemy.Enemy;

[SceneTree]
public partial class EnemyPanel : Control {
  private Enemy? CurrentEnemy = null;

  [OnInstantiate]
  private void Initialise() {

  }

  public void UpdateGui(Enemy enemy) {
    var pos_3d = enemy.GlobalPosition + enemy.GetGuiOffset();
    var cam = GetViewport().GetCamera3D();
    var pos_2d = cam.UnprojectPosition(pos_3d);
    GlobalPosition = pos_2d;
    Visible = !cam.IsPositionBehind(pos_3d);

    if (CurrentEnemy != enemy) {
      _.PanelContainer.VBoxContainer.Prompt.Set("bbcode", enemy.Prompt);
    }

    var currentInput = _.PanelContainer.VBoxContainer.Prompt.Get("bbcode").ToString();
    if (currentInput != enemy.Input) {
      _.PanelContainer.VBoxContainer.Input.Set("bbcode", enemy.Input);
    }
    // if (enemy.Active) {
    //   // _.PanelContainer.VBoxContainer.Input.Show();
    // }
    // else {
    //   // _.PanelContainer.VBoxContainer.Input.Hide();
    //   _.PanelContainer.VBoxContainer.Input.Set("bbcode", enemy.Input);
    // }

    CurrentEnemy = enemy;
  }
}
