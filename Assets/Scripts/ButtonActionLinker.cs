using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class ButtonActionLinker : MonoBehaviour
{
    [SerializeField] Action action;

    public void ExcecuteAction()
    {
        action.ExcecuteAction();
        gameObject.layer = 0;
    }

    public IEnumerator PressButton()
    {
        yield return transform.DOLocalMoveY(-1, 0.5f).WaitForKill();
        ExcecuteAction();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if(action != null) Gizmos.DrawLine(action.transform.position, transform.position);
    }
}
