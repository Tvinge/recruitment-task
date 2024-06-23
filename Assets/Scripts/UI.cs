using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    [SerializeField] TMP_InputField ipAdressInput;
    [SerializeField] TMP_InputField portInput;
    [SerializeField] ConnectionManager connectionManager;
    [SerializeField] Button button;

    public void SetIPAdress()
    {
        if (ipAdressInput.text != null)
        {
            connectionManager._connectIP = ipAdressInput.text;
            connectionManager._listenIP = ipAdressInput.text;
        }
        if (portInput.text != null)
        {
            connectionManager._port = ushort.Parse(portInput.text);
        }
        connectionManager.StartGame();
    }


}
