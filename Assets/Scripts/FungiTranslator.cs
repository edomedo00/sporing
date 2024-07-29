using DG.Tweening;
using extOSC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class FungiTranslator : Fungi
{
    [SerializeField] float talkMargin = 2;
    Transform player;
    public OSCTransmitter Transmitter;
    // Start is called before the first frame update

    public override void Awake()
    {
        base.Awake();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(JoinMyself());
    }


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


    //Aquí puedes poner el sonido de cuando el translator se encuentra al jugador. También puedes modificar este IEnumerator
    //para que se espere más, salte más o lo que tú quieras.
    IEnumerator JoinMyself()
    {
        yield return sequence.WaitForKill();
        while (Vector3.Distance(transform.position, player.position) > talkMargin)
            yield return null;

        var transpose = 100;
        var jumps = 3;
        var message = new OSCMessage("/fungiJump", OSCValue.Float(transpose), OSCValue.Int(jumps));
        Transmitter.Send(message);
        sequence = Talk();
        yield return sequence.WaitForKill();

        Transmitter.Send(new OSCMessage("/ampPiano"));
        JoinFungi();
    }


    //Aquí se encuentra toda la interacción cuando juntas un Fungi nuevo
    public IEnumerator JoinAnotherFungi(Collider other)
    {
        if (!IsInTheSameHeight(other.transform.parent, 1)) yield break;

        if (waypoints.Count == 0) yield break;
        if(interacting) yield break;
        interacting = true;
        Fungi otherFungi = other.transform.parent.gameObject.GetComponent<Fungi>();
        
        yield return StartCoroutine(RepositionInFrontOf(other.transform.parent));

        sequence = Talk();
        state = State.Walking;
        yield return sequence.WaitForKill();
        otherFungi.sequence = otherFungi.Talk();
        yield return otherFungi.sequence.WaitForKill();

        for(int i = 0; i < 3; i++)
        {
            sequence = Talk();
            otherFungi.sequence = otherFungi.Talk();
            yield return otherFungi.sequence.WaitForKill();
        }

        otherFungi.JoinFungi();
        FollowPlayer();
        interacting = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, talkMargin);
    }
}
