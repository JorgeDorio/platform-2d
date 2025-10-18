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
	private int _health = 3;

	public override void _Ready()
	{
		_targetPosition = _pointA;
		_slimeAnimation.Play("idle");
		_slimeAnimation.FlipH = true;
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
	}


	public void Die(float hitDirection)
	{
		if (_state == SlimeState.DIE) return;

		_health--;

		Vector2 hitVelocity = Velocity;
		hitVelocity.X = hitDirection * JumpHorizontalSpeed * 1.5f;
		hitVelocity.Y = JumpVelocity * 1.2f;
		Velocity = hitVelocity;

		_canJump = false;
		_jumpTimer.Stop();

		if (_health <= 0)
		{
			_state = SlimeState.DIE;
			_slimeAnimation.Play("die");
		}
		else
		{
			_slimeAnimation.Play("hit");
		}
	}

	public void OnAnimationFinished()
	{
		if (_state == SlimeState.DIE)
		{
			QueueFree();
		}
	}

	public override void _PhysicsProcess(double delta)
	{

		Vector2 velocity = Velocity;

		if (!IsOnFloor())
		{
			velocity.Y += GetGravity().Y * (float)delta;
		}
		else
		{
			if (velocity.Y >= 0)
			{
				if (_state == SlimeState.DIE)
				{
					velocity.X = 0;
				}
				else if (_canJump)
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

					if (_jumpTimer.IsStopped() && _state != SlimeState.DIE)
					{
						_slimeAnimation.Play("idle");
						_jumpTimer.Start();
					}
				}
			}
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	private void _on_timer_timeout()
	{
		if (_state != SlimeState.DIE)
		{
			_canJump = true;
		}
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