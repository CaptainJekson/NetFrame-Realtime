using NetFrame.Core;
using UnityEngine;

namespace ExamplesNew
{
    public class ClientManager : MonoBehaviour
    {
        private NetFrameClientNew _netFrameClientNew;
        
        private void Awake()
        {
            _netFrameClientNew = new NetFrameClientNew();
            _netFrameClientNew.Connect("127.0.0.1", 8080);
        }

        private void Update()
        {
            _netFrameClientNew.Run(0);

            if (Input.GetKeyDown(KeyCode.S))
            {
                _netFrameClientNew.SendTest();
            }
        }

        private void OnDestroy()
        {
            _netFrameClientNew.Stop();
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                Application.runInBackground = true;
            }
        }
    }
}