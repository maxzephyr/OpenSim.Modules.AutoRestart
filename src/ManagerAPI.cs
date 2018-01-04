using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

namespace OpenSim.Modules.AutoRestart
{
    class ManagerAPI
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
                String _data = "pass=" + _authPass.Trim() + "&trigger=" + _trigger.Trim() + "&uuid=" + _regionID.Trim() + "";
                m_log.Info("[ManagerAPI] Send Restart Command (URL: " + _adress + ") (Data: " + _data + ")");
                m_client.UploadStringAsync(new Uri(_adress), _data);
            }
            catch (Exception _error)
            {
                m_log.Error("[ManagerAPI] sendRestartCommand: " + _error.Message);
            }
        }

        public static void sendShutDownCommand(String _URL, String _authPass, String _trigger, String _regionID)
        {
            try
            {
                createClient();

                String _adress = "http://" + new Uri(_URL).Authority + "/opensim/stop";
                String _data = "pass=" + _authPass.Trim() + "&trigger=" + _trigger.Trim() + "&uuid=" + _regionID.Trim() + "";
                m_log.Info("[ManagerAPI] Send ShutDown Command (URL: " + _adress + ") (Data: " + _data + ")");
                m_client.UploadStringAsync(new Uri(_adress), _data);
            }
            catch(Exception _error)
            {
                m_log.Error("[ManagerAPI] sendShutDownCommand: " + _error.Message);
            }
        }
    }
}
