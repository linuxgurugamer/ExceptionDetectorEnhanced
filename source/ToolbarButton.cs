using KSP.UI.Screens;
using UnityEngine;

namespace ExceptionDetector
{
    [KSPAddon(KSPAddon.Startup.AllGameScenes, true)]
    public class ToolbarButton : MonoBehaviour
    {
        internal const string MODID = "ED_NS";
        internal const string MODNAME = "ExceptionDetector";
        //static IssueGUI instance = null;
        public static ToolbarButton toolbarButton = null;

        private static GameObject go;

        void Awake()
        {
            GameEvents.onGUIApplicationLauncherReady.Add(OnAppLauncherReady);
            DontDestroyOnLoad(this);
            toolbarButton = this;
        }

        private static readonly string TestsPassingIconLocation = "ExceptionDetector/Icons/ed";

        private ApplicationLauncherButton button;
        private Texture TestsPassingIcon;

        private ToolbarWrapper toolbar;
        private void OnAppLauncherReady()
        {
            TestsPassingIcon = GameDatabase.Instance.GetTexture(TestsPassingIconLocation, false);

            if (button != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(button);
                button = null;
            }
            toolbar = new ToolbarWrapper(
                this,
                OnTrue, OnFalse,
                ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.SPACECENTER |
                ApplicationLauncher.AppScenes.MAPVIEW | ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.TRACKSTATION,
                MODNAME,
                MODID,
                TestsPassingIconLocation,
                TestsPassingIconLocation,
                "Exception Detector"
                );
        }


        internal void OnTrue()
        {
            if (go == null)
            {
                go = new GameObject("Any");
            }
            if (!IssueGUI.isActive)
            {
                ExceptionDetector.fiGui = go.AddComponent<IssueGUI>();
            }
        }

        internal void OnFalse()
        {
            if (ExceptionDetector.fiGui != null)
            {
                Destroy(ExceptionDetector.fiGui);
                ExceptionDetector.fiGui = null;
                IssueGUI.isActive = false;
            }
        }

        internal void SetFalse(bool value = false)
        {
            toolbar.SetFalse(value);
        }

    }
}
