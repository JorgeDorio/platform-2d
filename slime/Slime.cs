using System;
using Godot;

enum SlimeState
{
	PATROL,
	CHASE,
	DIE
}

public partial class Slime : CharacterBody2D
{
	[Export]
	public AnimatedSprite2D _slimeAnimation;
	[Export]
	public CharacterBody2D _player;
	[Export]
	public Marker2D _pointA;
	[Export]
	public Marker2D _pointB;
	[Export]
	private Timer _jumpTimer;

	public const float JumpHorizontalSpeed = 80.0f;
	public const float JumpVelocity = -200.0f;

	private SlimeState _state = SlimeState.PATROL;
	private Marker2D _targetPosition;
	private float direction = -1;
	private bool _canJump = false;

	public override void _Ready()
	{
		_targetPosition = _pointA;
		_slimeAnimation.Play("idle");
	}

	public void UpdatePatrolLogic()
	{
		bool hasCrossedTarget = false;
		if (direction > 0 && GlobalPosition.X >= _targetPosition.GlobalPosition.X)
		{
			hasCrossedTarget = true;
		}
		else if (direction < 0 && GlobalPosition.X <= _targetPosition.GlobalPosition.X)
		{
			hasCrossedTarget = true;
		}

		if (hasCrossedTarget)
		{
			if (_targetPosition == _pointA)
			{
				_targetPosition = _pointB;
				direction = 1;
				_slimeAnimation.FlipH = false;
			}
			else
			{
				_targetPosition = _pointA;
				direction = -1;
				_slimeAnimation.FlipH = true;
			}
		}

		if (Math.Abs(GlobalPosition.X - _player.GlobalPosition.X) <= 150)
		{
			_state = SlimeState.CHASE;
		}
	}

	public void UpdateChaseLogic()
	{
		var distanceToPlayer = GlobalPosition.X - _player.GlobalPosition.X;
		direction = distanceToPlayer <= 0 ? 1 : -1;
		_slimeAnimation.FlipH = direction == -1;

		if (Math.Abs(distanceToPlayer) > 180)
		{
			_state = SlimeState.PATROL;
		}
	}


	public void Die()
	{
		_state = SlimeState.DIE;
		Velocity = Vector2.Zero;
		_slimeAnimation.Play("die");
		SetCollisionMaskValue(1, false);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_state == SlimeState.DIE) return;

		Vector2 velocity = Velocity;

		if (!IsOnFloor())
		{
			velocity.Y += GetGravity().Y * (float)delta;
		}
		else
		{
			if (_canJump)
			{
				if (_state == SlimeState.PATROL)
				{
					UpdatePatrolLogic();
				}
				else if (_state == SlimeState.CHASE)
				{
					UpdateChaseLogic();
				}

				velocity.Y = JumpVelocity;
				velocity.X = direction * JumpHorizontalSpeed;

				_canJump = false;
			}
			else
			{
				velocity.X = 0;

				if (_jumpTimer.IsStopped())
				{
					_slimeAnimation.Play("idle");
					_jumpTimer.Start();
				}
			}
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	private void _on_timer_timeout()
	{
		_canJump = true;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body.IsInGroup("player") && _state != SlimeState.DIE)
		{
			if (!body.IsQueuedForDeletion())
			{
				body.CallDeferred("Die");
			}
		}
	}
}