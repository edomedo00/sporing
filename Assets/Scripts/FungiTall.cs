using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FungiTall : FungiJump
{
    public override void NoTween(int times = 3, float speed = 0.2F)
    {
        base.NoTween(times, speed);
        // Aquí puedes poner el sonido de NO
    }

    public override Sequence JumpTween(int jumpNumber = 1)
    {
        return base.JumpTween(jumpNumber);
        // Aquí puedes poner el sonido de SALTO
    }

    public override Sequence Talk()
    {
        //Aquí puedes poner el sonido que van a hacer al HABLAR
        return base.JumpTween();
    }
}
