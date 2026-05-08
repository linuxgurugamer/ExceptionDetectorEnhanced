using ExceptionDetectorEnhanced.Support;
using UnityEngine;
using static KSP.UI.Screens.ApplicationLauncher;
using KSP.Localization;


namespace ExceptionDetectorEnhanced
{
    public class SettingsWindow : MonoBehaviour
    {
        private Rect windowRect = new Rect(300, 200, 300, 260);
        private bool showWindow = true;

        private const int WindowId = 987654;

        private GUIStyle labelStyle;
        private GUIStyle toggleStyle;

        void Start()
        {
            GameEvents.onShowUI.Add(OnShowUI);
            GameEvents.onHideUI.Add(OnHideUI);
        }

        void OnDestroy()
        {
            GameEvents.onShowUI.Remove(OnShowUI);
            GameEvents.onHideUI.Remove(OnHideUI);
        }

        private void OnGUI()
        {
            if (!showWindow)
                return;

            if (!ExceptionDetectorEnhanced.UseAltSkin && HighLogic.Skin != null)
                GUI.skin = HighLogic.Skin;

            windowRect = CBTWrapper.GUILayoutWindow(this.GetInstanceID(),
                windowRect,
                DrawWindow,
                Localizer.Format(Localizer.Format("#autoLOC_14945")),
                GUILayout.Width(300)
            );
        }

        private void DrawWindow(int id)
        {
            InitStyles();

            using (new GUILayout.VerticalScope())
            {
                GUILayout.Space(5);

                ExceptionDetectorEnhanced.WordWrap = GUILayout.Toggle(ExceptionDetectorEnhanced.WordWrap, Localizer.Format("#EXCD-22"), toggleStyle);
                ExceptionDetectorEnhanced.Bold = GUILayout.Toggle(ExceptionDetectorEnhanced.Bold, Localizer.Format("#EXCD-23"), toggleStyle);
                ExceptionDetectorEnhanced.UseWhitelist = GUILayout.Toggle(ExceptionDetectorEnhanced.UseWhitelist, Localizer.Format("#EXCD-24"), toggleStyle);
                ExceptionDetectorEnhanced.UseAlwayslist = GUILayout.Toggle(ExceptionDetectorEnhanced.UseAlwayslist, Localizer.Format("#EXCD-25"), toggleStyle);
                ExceptionDetectorEnhanced.UseAltSkin = GUILayout.Toggle(ExceptionDetectorEnhanced.UseAltSkin, Localizer.Format("#EXCD-26"), toggleStyle);
                ExceptionDetectorEnhanced.ShowAtStartup = GUILayout.Toggle(ExceptionDetectorEnhanced.ShowAtStartup, Localizer.Format("#EXCD-27"), toggleStyle);

                GUILayout.FlexibleSpace();

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button(Localizer.Format("#autoLOC_149410"), GUILayout.Width(80)))
                    {
                        Config.Save();
                        Destroy(this);
                    }
                    GUILayout.FlexibleSpace();
                }

            }
            GUI.DragWindow();
        }

        private void InitStyles()
        {
            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label)
                {
                    wordWrap = ExceptionDetectorEnhanced.WordWrap,
                    fontStyle = ExceptionDetectorEnhanced.Bold ? FontStyle.Bold : FontStyle.Normal
                };
            }

            if (toggleStyle == null)
            {
                toggleStyle = new GUIStyle(GUI.skin.toggle)
                {
                    fontStyle = ExceptionDetectorEnhanced.Bold ? FontStyle.Bold : FontStyle.Normal
                };
            }

            labelStyle.wordWrap = ExceptionDetectorEnhanced.WordWrap;
            labelStyle.fontStyle = ExceptionDetectorEnhanced.Bold ? FontStyle.Bold : FontStyle.Normal;
            toggleStyle.fontStyle = ExceptionDetectorEnhanced.Bold ? FontStyle.Bold : FontStyle.Normal;
        }

        public void OnShowUI()
        {
            showWindow = true;
        }

        public void OnHideUI()
        {
            showWindow = false;
        }
    }
}