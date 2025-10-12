using Godot;

public partial class Hitbox : Area2D
{
	public void OnAreaEntered()
	{
		GD.Print("Entrou");
	}
}
