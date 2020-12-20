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

        private int m_restartTime = 30;
        private int m_forceRestartTimeDays = 3;

        private List<String> m_disable = new List<String>();

        private DateTime forceRestartTime;

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
        }

        public void RemoveRegion(Scene scene)
        {

        }

        public void Initialise(IConfigSource config)
        {
            m_config = config;

            if(m_config.Configs["AutoRestart"] != null)
            {
                m_restartTime = m_config.Configs["AutoRestart"].GetInt("Time", 30);
                m_forceRestartTimeDays = m_config.Configs["AutoRestart"].GetInt("ForceRestartDays", 3);

                m_disable = new List<string>(m_config.Configs["AutoRestart"].GetString("DisableRegions", String.Empty).ToLower().Trim().Split(','));
            }

            m_timer = new System.Timers.Timer(60000);
            m_timer.Elapsed += new ElapsedEventHandler(timerEvent);
            m_timer.Start();

            forceRestartTime = DateTime.Now.AddDays(m_forceRestartTimeDays);

            m_log.Warn("[AutoRestart] Enable AutoRestart with a time of " + m_restartTime.ToString());
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
                    }
                }

                if (agentCount == 0 || DateTime.Compare(DateTime.Now, forceRestartTime) > 0)
                {
                    if (DateTime.Compare(DateTime.Now, forceRestartTime) > 0) {
                        m_log.Warn("[AutoRestart] Current time is after force restart time: " + forceRestartTime + ". Forcing restart");
                    }
                    m_log.Warn("[AutoRestart] Restart/Shutdown Region.");

                    foreach (Scene s in m_scene)
                    {
                        s.Backup(true);
                    }

                    Thread.Sleep(2000);
                    Environment.Exit(0);
                }
                else
                {
                    m_restartCounter = m_restartTime - 3;
                    m_log.Info("[AutoRestart] REGION IS NOT EMPTRY! MOVE RESTART.");
                }
            } else {
                                int timeLeft = m_restartTime - m_restartCounter;
                                String timeLeftString = "";
                                if (timeLeft == 1) {
                                        timeLeftString = timeLeft.ToString() + " minute";
                                } else {
                                        timeLeftString = timeLeft.ToString() + " minutes";
                                }
                                m_log.Info("[AutoRestart] Restart in " + timeLeftString + " (every " + m_restartTime.ToString() + " minutes). Force restart after " + forceRestartTime);
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
