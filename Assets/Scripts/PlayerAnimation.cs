using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using extOSC;


public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] Transform model;
    Sequence sequence;
    bool jumping;
    Vector3 originalLocalPos;
    public OSCTransmitter Transmitter;


    private void Start()
    {
        sequence = DOTween.Sequence();
        originalLocalPos = model.localPosition;
        jumping = false;
    }

    public void Jump(Vector3 velocity)
    {
        if (jumping) return;
        if (velocity.magnitude < 0.01f) return;
        Transmitter.Send(new OSCMessage("/walk"));
        StartCoroutine(JumpCoroutine());
    }

    IEnumerator JumpCoroutine()
    {
        jumping = true;
        sequence = DOTween.Sequence();
        sequence.Insert(0, model.DOLocalJump(originalLocalPos, 0.3f, 1, 0.3f));
        yield return sequence.WaitForKill();
        jumping = false;
    }
}
