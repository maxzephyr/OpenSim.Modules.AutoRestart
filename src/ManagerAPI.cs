using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace OpenSim.Modules.AutoRestart
{
    class ManagerAPI
    {
        private static WebClient m_client = null;

        private static void createClient()
        {
            if(m_client == null)
            {
                m_client = new WebClient();
            }
        }

        public static void sendRestartCommand(String _URL, String _authPass, String _trigger, String _regionID)
        {
            try
            {
                createClient();

                String _adress = "http://" + new Uri(_URL).Authority + "/opensim/restart";
                String _data = "pass:" + _authPass.Trim() + "&trigger:" + _trigger.Trim() + "&uuid:" + _regionID.Trim() + "";
                m_client.UploadStringAsync(new Uri(_adress), _data);
            }catch
            {

            }
        }

        public static void sendShutDownCommand(String _URL, String _authPass, String _trigger, String _regionID)
        {
            try
            {
                createClient();

                String _adress = "http://" + new Uri(_URL).Authority + "/opensim/stop";
                String _data = "pass:" + _authPass.Trim() + "&trigger:" + _trigger.Trim() + "&uuid:" + _regionID.Trim() + "";
                m_client.UploadStringAsync(new Uri(_adress), _data);
            }
            catch
            {

            }
        }
    }
}
