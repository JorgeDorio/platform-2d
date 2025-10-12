using Godot;
using System;

public partial class Coin : Area2D
{
	[Export]
	public AnimatedSprite2D _coinAnimation;

	public float Direction { get; set; } = 1;

	public const float Speed = 500.0f;

	public override void _Ready()
	{
		_coinAnimation.Play("idle");
	}


	public override void _Process(double delta)
	{
		var position = new Vector2
		{
			X = Speed * (float)delta * Direction,
		};
		Position += position;
	}

	public void OnExitScreen()
	{
		QueueFree();
	}

	public void OnBodyEntered(Area2D other)
    {
		GD.Print("Entrou no outro!");
    }
}
