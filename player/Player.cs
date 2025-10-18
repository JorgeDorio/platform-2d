using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export]
	public AnimatedSprite2D _playerAnimation;
	[Export]
	public AnimationPlayer _playerAnimator;
	[Export]
	public PackedScene _projectile;
	[Export]
	public Marker2D _shootPosition;

	[Export]
	public CollisionShape2D _collisionWalk;
	[Export]
	public CollisionShape2D _collisionAttack;

	public const float Speed = 300.0f;
	public const float JumpVelocity = -400.0f;

	private float _direction = 1;
	private bool _isAttacking = false;

	private float _originalWalkPosX;
	private float _originalWalkRotation;
	private float _originalAttackPosX;
	private float _originalShootPosX;
	private float _originalAttackRotation;

	public override void _Ready()
	{
		_playerAnimator.Connect(AnimationMixer.SignalName.AnimationFinished, new Callable(this, nameof(OnAnimationFinished)));

		_originalWalkPosX = Math.Abs(_collisionWalk.Position.X);
		_originalWalkRotation = Math.Abs(_collisionWalk.Rotation);
		_originalAttackPosX = Math.Abs(_collisionAttack.Position.X);
		_originalShootPosX = Math.Abs(_shootPosition.Position.X);
		_originalAttackRotation = Math.Abs(_collisionAttack.Rotation);
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		if (!IsOnFloor())
		{
			velocity.Y += GetGravity().Y * (float)delta;
		}

		if (Input.IsActionJustPressed("attack") && !_isAttacking && IsOnFloor())
		{
			Attack();
		}

		if (_isAttacking)
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
		}
		else
		{
			if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
			{
				velocity.Y = JumpVelocity;
			}

			Vector2 inputDirection = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
			if (inputDirection.X != 0)
			{
				_direction = inputDirection.X;
				velocity.X = _direction * Speed;
			}
			else
			{
				velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			}
		}

		UpdateFlip();

		if (Input.IsActionJustPressed("ui_fire"))
		{
			Shoot();
		}

		Velocity = velocity;
		AnimationHandler();
		MoveAndSlide();
	}

	private void UpdateFlip()
	{
		_playerAnimation.FlipH = _direction < 0;

		Vector2 shootPos = _shootPosition.Position;
		shootPos.X = _originalShootPosX * _direction;
		_shootPosition.Position = shootPos;

		Vector2 walkPos = _collisionWalk.Position;
		walkPos.X = _originalWalkPosX * _direction;
		_collisionWalk.Position = walkPos;
		_collisionWalk.Rotation = _originalWalkRotation * _direction;

		Vector2 attackPos = _collisionAttack.Position;
		attackPos.X = _originalAttackPosX * _direction;
		_collisionAttack.Position = attackPos;
		_collisionAttack.Scale = new Vector2(_direction, 1);
	}

	public void Attack()
	{
		_isAttacking = true;
	}

	public void OnAnimationFinished(StringName animName)
	{
		if (animName == "attack")
		{
			_isAttacking = false;
		}
	}

	public void OnHitEnemy(Area2D area)
	{
		area.GetParent().CallDeferred("Die", _direction);
	}

	public void AnimationHandler()
	{
		string newAnim = "idle";

		if (_isAttacking)
		{
			newAnim = "attack";
		}
		else if (!IsOnFloor())
		{
		}
		else if (Velocity.X != 0)
		{
			newAnim = "walk";
		}

		if (_playerAnimator.CurrentAnimation != newAnim)
		{
			_playerAnimator.Play(newAnim);
		}
	}

	private void Shoot()
	{
		if (_projectile == null || _shootPosition == null) return;

		var projectileInstance = _projectile.Instantiate<Node2D>();

		if (projectileInstance.HasMethod("SetDirection"))
		{
			projectileInstance.Call("SetDirection", _direction);
		}

		projectileInstance.GlobalPosition = _shootPosition.GlobalPosition;
		GetTree().Root.AddChild(projectileInstance);
	}

	public void Die()
	{
	}
}