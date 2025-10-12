using Godot;
using System;

public partial class SlimeAnimation : AnimatedSprite2D
{
	public void OnAnimationFinished()
    {
        GD.Print("Animation finished!");
    }

}
