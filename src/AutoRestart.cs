using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenSim.Region.Framework.Scenes;
using Nini.Config;
using log4net;
using System.Reflection;
using OpenSim.Region.Framework.Interfaces;
using System.Timers;
using Mono.Addins;
using OpenSim.Framework;
using System.Threading;

[assembly: Addin("AutoRestart", "1.0")]
[assembly: AddinDependency("OpenSim.Region.Framework", OpenSim.VersionInfo.VersionNumber)]

namespace OpenSim.Modules.AutoRestart
{
    [Extension(Path = "/OpenSim/RegionModules", NodeName = "RegionModule")]
    class AutoRestart : ISharedRegionModule
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private List<Scene> m_scene = new List<Scene>();
        private System.Timers.Timer m_timer;
        private int m_restartCounter = 0;
        private IConfigSource m_config;

        private bool m_sendManagerShutdownCommand = false;
        private String m_managerURL = "";
        private String m_managerPass = "";
        private String m_managerTrigger = "";
        private int m_restartTime = 30;

        private List<String> m_disable = new List<String>();

        #region IRegionModule interface

        public void RegionLoaded(Scene scene)
        {
            m_scene.Add(scene);
        }

        public void AddRegion(Scene scene)
        {
            if (m_disable.Contains(scene.RegionInfo.RegionName))
                return;

            m_scene.Add(scene);

            m_timer = new System.Timers.Timer(60000);
            m_timer.Elapsed += new ElapsedEventHandler(timerEvent);
            m_timer.Start();

            m_log.Warn("[AutoRestart] Enable AutoRestart with a time of " + m_restartTime.ToString() + "(Shutdown: " + m_sendManagerShutdownCommand.ToString() + ")");
        }

        public void RemoveRegion(Scene scene)
        {

        }

        public void Initialise(IConfigSource config)
        {
            m_config = config;

            if(m_config.Configs["AutoRestart"] != null)
            {
                m_managerURL = m_config.Configs["AutoRestart"].GetString("ManagerURL", String.Empty);
                m_managerPass = m_config.Configs["AutoRestart"].GetString("ManagerPass", String.Empty);
                m_managerTrigger = m_config.Configs["AutoRestart"].GetString("ManagerTrigger", String.Empty);
                m_sendManagerShutdownCommand = m_config.Configs["AutoRestart"].GetBoolean("RegionShutDown", false);
                m_restartTime = m_config.Configs["AutoRestart"].GetInt("Time", 30);

                m_disable = new List<string>(m_config.Configs["AutoRestart"].GetString("DisableRegions", String.Empty).ToLower().Trim().Split(','));
            }
        }

        public void timerEvent(object sender, ElapsedEventArgs e)
        {
            m_restartCounter++;

            if (m_restartCounter >= m_restartTime)
            {
                int agentCount = 0;

                foreach (Scene s in m_scene)
                {
                    if (s.GetRootAgentCount() != 0)
                    {
                        List<ScenePresence> _pres = s.GetScenePresences();

                        foreach (ScenePresence _p in _pres)
                        {
                            if (_p.PresenceType == PresenceType.User)
                            {
                                agentCount = agentCount + s.GetRootAgentCount();
                            }
                        }

                        s.Backup(true);
                    }
                }

                if (agentCount == 0)
                {
                    m_log.Warn("[AutoRestart] Restart/Shutdown Region.");

                    if (m_sendManagerShutdownCommand == true)
                    {
                        ManagerAPI.sendShutDownCommand(m_managerURL, m_managerPass, m_managerTrigger, m_scene[0].RegionInfo.RegionID.ToString());
                    }
                    else
                    {
                        ManagerAPI.sendRestartCommand(m_managerURL, m_managerPass, m_managerTrigger, m_scene[0].RegionInfo.RegionID.ToString());
                    }
                }
                else
                {
                    m_restartCounter = 0;
                    m_log.Info("[AutoRestart] REGION IS NOT EMPTRY! MOVE RESTART.");
                }
            }
        }

        public void PostInitialise()
        {

        }

        public void Close()
        {
        }

        public string Name
        {
            get { return "AutoRestart"; }
        }

        public Type ReplaceableInterface
        {
            get { return null; }
        }


        #endregion
    }
}
