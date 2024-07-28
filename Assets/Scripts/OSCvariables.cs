using extOSC;
using UnityEngine;


public class OSCvariables : MonoBehaviour
{
    #region Public Vars

    public OSCTransmitter Transmitter;

    #endregion

    #region Private Vars

    private const string _waterAddress1 = "/waterProximity1";
    private const string _waterAddress2 = "/waterProximity2";


    #endregion

    #region Public Methods

    public void SendDistanceRiver(float value)
    {
        Send(_waterAddress1, OSCValue.Float(value));

    }
    public void SendDistanceRiver1(float value)
    {
        Send(_waterAddress2, OSCValue.Float(value));

    }

    #endregion

    #region Private Methods
    private void Send(string address, OSCValue value)
    {
        var message = new OSCMessage(address, value);

        Transmitter.Send(message);
    }

    #endregion
}


