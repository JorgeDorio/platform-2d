using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export]
	public AnimatedSprite2D _playerAnimation;
	[Export]
	public PackedScene _projectile;
	[Export]
	public Marker2D _shootPosition;

	public const float Speed = 300.0f;
	public const float JumpVelocity = -400.0f;
	private float _direction = 1;

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		_direction = direction.X == 0 ? _direction : direction.X;
		if (direction != Vector2.Zero)
		{
			_playerAnimation.FlipH = _direction < 0;
			Vector2 shootPosition = _shootPosition.Position;
			var position = Math.Abs(shootPosition.X);
			shootPosition.X = position * _direction;
			_shootPosition.Position = shootPosition;
			_playerAnimation.Play("walk");
			velocity.X = direction.X * Speed;
		}
		else
		{
			_playerAnimation.Play("idle");
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
		}

		if (Input.IsActionJustPressed("ui_fire"))
		{
			Shoot();
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	public void Die()
	{
		QueueFree();
	}

	private void Shoot()
	{
		if (_projectile == null || _shootPosition == null)
		{
			return;
		}

		var projectileInstance = _projectile.Instantiate<Coin>();
		projectileInstance.Direction = _direction;
		projectileInstance.GlobalPosition = _shootPosition.GlobalPosition;
		GetTree().Root.AddChild(projectileInstance);
	}
}
