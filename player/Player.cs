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
	private bool _isAttacking = false;

	public override void _Ready()
	{
		_playerAnimation.Connect(AnimatedSprite2D.SignalName.AnimationFinished, new Callable(this, nameof(OnAnimationFinished)));
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// Aplica a gravidade (isso acontece independentemente de estar atacando ou não)
		if (!IsOnFloor())
		{
			velocity.Y += GetGravity().Y * (float)delta;
		}

		// Verifica o input de ataque primeiro
		if (Input.IsActionJustPressed("attack") && !_isAttacking && IsOnFloor())
		{
			Attack();
		}

		// **LÓGICA PRINCIPAL: Decide o que fazer baseado no estado _isAttacking**
		if (_isAttacking)
		{
			// Se está atacando, a velocidade horizontal deve ir para zero.
			// Usar MoveToward cria uma parada suave em vez de um corte brusco.
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
		}
		else
		{
			// Se NÃO está atacando, processa os inputs de movimento normalmente.

			// Pulo
			if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
			{
				velocity.Y = JumpVelocity;
			}

			// Movimento Horizontal
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

		// Atualiza a direção do sprite e do ponto de tiro (sempre)
		_playerAnimation.FlipH = _direction < 0;
		Vector2 shootPos = _shootPosition.Position;
		shootPos.X = Math.Abs(shootPos.X) * _direction;
		_shootPosition.Position = shootPos;

		// Lógica de tiro (pode acontecer mesmo parado)
		if (Input.IsActionJustPressed("ui_fire"))
		{
			Shoot();
		}

		// Aplica a velocidade, animações e movimento
		Velocity = velocity;
		AnimationHandler();
		MoveAndSlide();
	}

	public void Attack()
	{
		_isAttacking = true;
		// Não é mais necessário zerar a velocidade aqui, pois o _PhysicsProcess já cuida disso.
	}

	public void OnAnimationFinished()
	{
		if (_playerAnimation.Animation == "attack")
		{
			_isAttacking = false;
		}
	}

	public void AnimationHandler()
	{
		string newAnim = "idle";

		// A lógica de animação é a última coisa a ser decidida, baseada no estado final
		if (_isAttacking)
		{
			newAnim = "attack";
		}
		else if (!IsOnFloor())
		{
			newAnim = "jump"; // Sugestão: adicione uma animação de pulo
		}
		else if (Velocity.X != 0)
		{
			newAnim = "walk";
		}

		if (_playerAnimation.Animation != newAnim)
		{
			_playerAnimation.Play(newAnim);
		}
	}

	public void Die()
	{
		QueueFree();
	}

	private void Shoot()
	{
		if (_projectile == null || _shootPosition == null) return;

		var projectileInstance = _projectile.Instantiate<Coin>();
		projectileInstance.Direction = _direction;
		projectileInstance.GlobalPosition = _shootPosition.GlobalPosition;
		GetTree().Root.AddChild(projectileInstance);
	}
}