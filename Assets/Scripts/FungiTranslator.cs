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
    // Start is called before the first frame update

    public override void Awake()
    {
        base.Awake();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(JoinMyself());
    }


    public override void NoTween(int times = 3, float speed = 0.2F)
    {
        Transmitter.Send(new OSCMessage("/translatorNo"));
        base.NoTween(times, speed); 
        
    }

    public override Sequence JumpTween(int jumpNumber = 1)
    {
        var transpose = 100;
        var message = new OSCMessage("/fungiJump", OSCValue.Float(transpose), OSCValue.Int(jumpNumber));
        Transmitter.Send(message);

        return base.JumpTween(jumpNumber);
    }

    public override Sequence Talk()
    {
        //Aquí puedes poner el sonido que van a hacer al HABLAR
        Transmitter.Send(new OSCMessage("/translatorTalk"));
        return base.JumpTween();
    }


    //Aquí puedes poner el sonido de cuando el translator se encuentra al jugador. También puedes modificar este IEnumerator
    //para que se espere más, salte más o lo que tú quieras.
    IEnumerator JoinMyself()
    {
        yield return sequence.WaitForKill();
        while (Vector3.Distance(transform.position, player.position) > talkMargin)
            yield return null;

        sequence = Talk();
        yield return sequence.WaitForKill();
        yield return new WaitForSeconds(2.5f); // Cambia 2f por el tiempo que desees esperar

        for (int i = 0; i < 3; i++) {
            sequence = JumpTween();
            yield return sequence.WaitForKill();
        }
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

        state = State.Walking;
        sequence = Talk();
        yield return sequence.WaitForKill();
        yield return new WaitForSeconds(2.5f); 

        otherFungi.sequence = otherFungi.Talk();
        yield return otherFungi.sequence.WaitForKill();
        yield return new WaitForSeconds(2.5f); 

        for (int i = 0; i < 3; i++)
        {
            sequence = JumpTween();
            yield return new WaitForSeconds(0.05f); 
            otherFungi.sequence = otherFungi.JumpTween();
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
