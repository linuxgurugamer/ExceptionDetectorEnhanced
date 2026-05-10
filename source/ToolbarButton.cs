using KSP.UI.Screens;
using UnityEngine;
using static GameEvents;

namespace ExceptionDetectorEnhanced
{
    [KSPAddon(KSPAddon.Startup.AllGameScenes, true)]
    public class ToolbarButton : MonoBehaviour
    {
        internal const string MODID = "ED_NS";
        internal const string MODNAME = "ExceptionDetectorEnhanced";
        public static ToolbarButton toolbarButton = null;

        private static GameObject go;

        void Awake()
        {
            GameEvents.onGUIApplicationLauncherReady.Add(OnAppLauncherReady);
            GameEvents.onGameSceneSwitchRequested.Add(onGameSceneSwitchRequested);
            DontDestroyOnLoad(this);
            toolbarButton = this;
        }
        void OnDestroy()
        {
            GameEvents.onGUIApplicationLauncherReady.Remove(OnAppLauncherReady);
            GameEvents.onGameSceneSwitchRequested.Remove(onGameSceneSwitchRequested);
        }

        void onGameSceneSwitchRequested(GameEvents.FromToAction<GameScenes, GameScenes> data)
        {
            toolbar.Destroy();
        }

        private static readonly string TestsPassingIconLocation = "ExceptionDetectorEnhanced/Icons/ed";

        private Texture TestsPassingIcon;

        private ToolbarWrapper toolbar;
        private void OnAppLauncherReady()
        {
            TestsPassingIcon = GameDatabase.Instance.GetTexture(TestsPassingIconLocation, false);

            //if (button != null)
            //{
            //    ApplicationLauncher.Instance.RemoveModApplication(button);
            //    button = null;
            //}
            if (toolbar == null || ToolbarWrapper.toolbarControllerAvailable == false)
            {
                toolbar = new ToolbarWrapper(
                    this,
                    OnTrue, OnFalse,
                    ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.SPACECENTER |
                    ApplicationLauncher.AppScenes.MAPVIEW | ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.TRACKSTATION,
                    MODNAME,
                    MODID,
                    TestsPassingIconLocation,
                    TestsPassingIconLocation,
                    "Exception Detector Enhanced"
                    );
            }
        }

        internal void OnTrue()
        {
            if (go == null)
            {
                go = new GameObject("Any");
            }
            if (!IssueGUI.isActive)
            {
                ExceptionDetectorEnhanced.fiGui = go.AddComponent<IssueGUI>();
            }
        }

        internal void OnFalse()
        {
            if (ExceptionDetectorEnhanced.fiGui != null)
            {
                Destroy(ExceptionDetectorEnhanced.fiGui);
                ExceptionDetectorEnhanced.fiGui = null;
                IssueGUI.isActive = false;
            }
        }

        internal void SetFalse(bool value = false)
        {
            toolbar.SetFalse(value);
        }

    }
}
