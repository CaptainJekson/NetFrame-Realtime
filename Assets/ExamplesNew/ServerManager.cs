using NetFrame.Core;
using UnityEngine;

namespace ExamplesNew
{
    public class ServerManager : MonoBehaviour
    {
        private NetFrameServerNew _netFrameServerNew;
        
        private void Awake()
        {
            _netFrameServerNew = new NetFrameServerNew();
            _netFrameServerNew.Start(8080, 10);
        }

        private void Update()
        {
            _netFrameServerNew.Run(15);

            if (Input.GetKeyDown(KeyCode.S))
            {
                _netFrameServerNew.SendAllTest();
            }
        }

        private void OnDestroy()
        {
            _netFrameServerNew.Stop();
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