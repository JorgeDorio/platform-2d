using Godot;
using System;

public partial class Slime : CharacterBody2D
{
	[Export]
	public AnimatedSprite2D _slimeAnimation;

	public const float Speed = 300.0f;
	public const float JumpVelocity = -400.0f;

	public override void _Ready()
	{
		_slimeAnimation.Play("idle");
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	private void _on_body_entered(Node2D body)
	{
		GD.Print("Entrou");
		if (body.IsInGroup("player"))
		{
			body.CallDeferred("Die");
		}
	}
}
