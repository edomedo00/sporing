using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action : MonoBehaviour
{
    protected bool excecuting;

    // Start is called before the first frame update
    public virtual void ExcecuteAction()
    {
        if (excecuting) return;
    }
}
