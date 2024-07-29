using DG.Tweening;
using extOSC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FungiTall : FungiJump
{
    public override void NoTween(int times = 3, float speed = 0.2F)
    {
        Transmitter.Send(new OSCMessage("/platformNo"));
        base.NoTween(times, speed);
    }

    public override Sequence JumpTween(int jumpNumber = 1)
    {
        var transpose = -200;
        var message = new OSCMessage("/fungiJump", OSCValue.Float(transpose), OSCValue.Int(jumpNumber));
        Transmitter.Send(message);
        return base.JumpTween(jumpNumber);
    }

    public override Sequence Talk()
    {
        Transmitter.Send(new OSCMessage("/platformTalk"));
        Transmitter.Send(new OSCMessage("/ampPulse"));
        return base.JumpTween();
    }
}
