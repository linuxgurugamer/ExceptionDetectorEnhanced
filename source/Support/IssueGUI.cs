// 
//     Copyright (C) 2014 CYBUTEK
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 

#region Using Directives

using ExceptionDetectorEnhanced.Support;
using KSP.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

#endregion

namespace ExceptionDetectorEnhanced
{
    public class IssueGUI : MonoBehaviour
    {
        #region Fields

        static DictionaryEditorWindow dew = null;
        private static GUIStyle buttonStyle;
        GUIStyle rightLabel;
        private bool hasPositioned;
        private List<string> message, logLine;
        private int msgCount = 20;
        internal Rect position; // = new Rect(Screen.width * .8f, Screen.height * .1f, Screen.width * .5f, Screen.height * 0.25f);
        private string title;
        private static GUIStyle titleStyle;
        private static GUIStyle listStyle;
        private bool initDone = false;
        private float lastFrameTime = 0.0f;

        public static bool isActive = false;

        #endregion

        #region Properties

        public bool HasBeenUpdated { get; set; }

        #endregion

        #region Methods: protected

        protected void Initmessage()
        {
            Config.Load();
            message = new List<string>();
            logLine = new List<string>();
            for (int x = 1; x <= msgCount; x++)
            {
                message.Add(String.Format("{0}: ", x));
                logLine.Add(String.Format("{0}: ", x));
            }
        }

        protected void Awake()
        {
            try
            {
                //DontDestroyOnLoad(this);
                Initmessage();
            }
            catch (Exception ex)
            {
                //Logger.Exception(ex);
            }
            //Logger.Log("FirstRunGui was created.");
        }

        protected void OnDestroy()
        {
            //Logger.Log("FirstRunGui was destroyed.");
            GameEvents.onShowUI.Remove(OnShowUI);
            GameEvents.onHideUI.Remove(OnHideUI);
        }

        float lastHeight = 0;
        bool isVisible = false;
        public void OnGUI()
        {
            try
            {
                if (!initDone)
                {
                    InitialiseStyles();
                }
                if (this.position.height > Screen.height * 0.85f)
                {
                    doScrollView = true;
                    this.hasPositioned = false;
                    this.position.height = Screen.height * 0.85f;
                }
                if (lastHeight != Screen.height)
                {
                    doScrollView = false;
                    lastHeight = Screen.height;
                }
                if (!isVisible)
                    this.position = CBTWrapper.GUILayoutWindow(this.GetInstanceID(), this.position, this.Window, this.title, HighLogic.Skin);
                //this.PositionWindow();
            }
            catch (Exception ex)
            {
                //Logger.Exception(ex);
            }
        }

        bool doScrollView = false;
        private void Update()
        {

            if (lastFrameTime < Time.time + 1)
            {
                lastFrameTime = Time.time;
                UpdateMessages();
            }

            ResizerUpdate();
        }

        private void UpdateMessages()
        {
            if (ExceptionDetectorEnhanced.ExceptionCount.Count() > 2)
            {
                var list = ExceptionDetectorEnhanced.ExceptionCount.ToList();
                logLine = Enumerable.Repeat("", msgCount).ToList();
                list.Sort((x, y) => y.Value.CompareTo(x.Value));
                for (int x = 0; x < msgCount; x++)
                {
                    message[x] = x >= list.Count() ? String.Format("{0}", x + 1) : String.Format("{0}:  {1} times : {2}", x + 1, list[x].Value, list[x].Key);

                    logLine[x] = x >= list.Count() ? String.Format("{0}", x + 1) : String.Format("{0}", list[x].Key);
                }
            }
        }

        protected void Start()
        {
            try
            {
                this.title = Localizer.Format("#EXCD-name") + " " + Localizer.Format("#EXCD-abbv");

                GameEvents.onShowUI.Add(OnShowUI);
                GameEvents.onHideUI.Add(OnHideUI);
                this.position = ExceptionDetectorEnhanced.position;
                if (position.width == 0 || position.height == 0)
                {
                    position.width = Screen.width * .5f;
                    position.height = Screen.height * 0.25f;
                    position.x = (Screen.width - position.width) / 2;
                    position.y = Screen.height * .1f; // (Screen.height - position.height)/ 2;
                }

            }
            catch (Exception ex)
            {
                //Logger.Exception(ex);
            }
        }

        #endregion

        #region Methods: private

        private void OnShowUI()
        {
            isVisible = false;
        }

        private void OnHideUI()
        {
            isVisible = true;
        }

        private void InitialiseStyles()
        {
            initDone = true;
            titleStyle = new GUIStyle(HighLogic.Skin.label)
            {
                normal =
                     {
                          textColor = Color.white
                     },
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                stretchWidth = true
            };
            listStyle = new GUIStyle(HighLogic.Skin.label)
            {
                normal =
                     {
                          textColor = Color.white
                     },
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                stretchWidth = true
            };

            buttonStyle = new GUIStyle(HighLogic.Skin.button)
            {
                normal =
                     {
                          textColor = Color.white
                     }
            };

            InitRightLabel();

        }

        void InitRightLabel()
        {
            rightLabel = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleRight
            };
        }

        const int MAXSTRINGLEN = 400;
        string TruncateWithEllipsis(string text, GUIStyle style, float maxWidth)
        {
            if (text.Length > MAXSTRINGLEN)
                text = text.Substring(0, MAXSTRINGLEN);
            if (style.CalcSize(new GUIContent(text)).x <= maxWidth)
                return text;

            string ellipsis = "...";
            float ellipsisWidth = style.CalcSize(new GUIContent(ellipsis)).x;

            for (int i = text.Length - 1; i > 0; i--)
            {
                string truncated = text.Substring(0, i);
                float width = style.CalcSize(new GUIContent(truncated)).x;

                if (width + ellipsisWidth <= maxWidth)
                    return truncated + ellipsis;
            }

            return ellipsis;
        }

        Vector2 curPos = new Vector2();
        private void Window(int id)
        {
            string togglespace = ExceptionDetectorEnhanced.UseAltSkin ? "" : "   ";
            try
            {
                using (new GUILayout.VerticalScope())
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button(" " + Localizer.Format("#EXCD-20") + " "))
                        {
                            dew = gameObject.AddComponent<DictionaryEditorWindow>();
                        }

                        if (GUILayout.Button(" " + Localizer.Format("#autoLOC_149458") + " "))
                        {
                            gameObject.AddComponent<SettingsWindow>();
                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("Click on a line to copy to clipboard", titleStyle);
                        GUILayout.FlexibleSpace();
                        GUILayout.Label(Localizer.Format("#EXCD-12"), titleStyle, GUILayout.MinWidth(120)); // Resizable Window
                    }
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label(Localizer.Format("#EXCD-05", msgCount), titleStyle, GUILayout.Width(Screen.width * 0.2f));
                        GUILayout.FlexibleSpace();

                        GUILayout.Label(Localizer.Format("#EXCD-13"), rightLabel, GUILayout.MinWidth(400)); // drag the right edge, bottom edge, or bottom-right corner
                    }
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label(Localizer.Format("#EXCD-06", Path.GetFullPath(ExceptionDetectorEnhanced.LogFile)), titleStyle); //, GUILayout.Width(position.width));
                        GUILayout.FlexibleSpace();
                    }
                    if (doScrollView)
                        curPos = GUILayout.BeginScrollView(curPos, false, true);
                    for (int x = 0; x < msgCount; x++)
                    {
                        if (message[x].Length > 5)
                        {
                            listStyle.wordWrap = ExceptionDetectorEnhanced.WordWrap;
                            listStyle.clipping = TextClipping.Clip;
                            listStyle.fontStyle = ExceptionDetectorEnhanced.Bold ? FontStyle.Bold : FontStyle.Normal;
                            bool rc = false;
                            if (!ExceptionDetectorEnhanced.WordWrap)
                                rc = GUILayout.Button(TruncateWithEllipsis(message[x], listStyle, position.width - 20), listStyle);
                            else
                                rc = GUILayout.Button(message[x], listStyle);
                            if (rc)
                            {
                                Debug.Log("ExceptionDetector, button pressed");

                                if (logLine[x].StartsWith(Localizer.Format("#EXCD-abbv")))
                                    logLine[x] = logLine[x].Substring(Localizer.Format("#EXCD-abbv").Length).TrimStart();
                                GUIUtility.systemCopyBuffer = logLine[x];
                            }

                        }
                    }
                    if (doScrollView)
                        GUILayout.EndScrollView();
                }
                GUILayout.FlexibleSpace();
                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(Localizer.Format("#autoLOC_149410"), buttonStyle))
                    {
                        Config.Save();
                        isActive = false;

                        if (ToolbarButton.toolbarButton != null)
                        {
                            ToolbarButton.toolbarButton.OnFalse();
                            ToolbarButton.toolbarButton.SetFalse();
                        }

                        if (dew != null)
                        {
                            dew.Destroy();
                            dew = null;
                        }
                        Destroy(this);

                    }
                    if (GUILayout.Button(Localizer.Format("#autoLOC_900305"), buttonStyle))
                    {
                        ExceptionDetectorEnhanced.ResetLists();
                        Initmessage();
                        doScrollView = false;
                    }
                }
                if (resizing == CursorType.Default)
                    GUI.DragWindow();
            }
            catch (Exception ex)
            {
                //Logger.Exception(ex);
            }
        }

        #endregion

        #region Resizer

        // Resize windows in the Update
        /// <summary>
        /// Defines available cursor types
        /// </summary>
        public enum CursorType
        {
            ResizeNS,
            ResizeEW,
            ResizeNSEW,
            Default
        }
        private Vector2 originalMousePosition;
        private Boolean windowLocked = false;
        internal CursorType resizing = CursorType.Default;
        Rect originalWindow;
        private readonly int minimumHeight = 300;
        private readonly int minimumWidth = 300;
        private Dictionary<String, Texture> buttonTextures = new Dictionary<String, Texture>();
        private readonly String modDir = "ExceptionDetectorEnhanced";
        string activeResizeWindow = "";
        bool updated = false;

        void CheckForResize(string winName, ref Rect windowRect)
        {
            if (updated)
                return;
            // Fix reversed y position in mouse coordinates
            Vector3 mousePos = Input.mousePosition;
            mousePos.y = Screen.height - mousePos.y;
            //mousePos.x = Screen.width - mousePos.x;

            /*** Resizing ***/

            //Boolean cursorInVZone = new Rect(windowRect.x, windowRect.yMax - 5, windowRect.width, 5).Contains(mousePos);
            //Boolean cursorInHZone = new Rect(windowRect.xMax - 5, windowRect.y, 5, windowRect.height).Contains(mousePos);

            Boolean cursorInVZone = false;
            Boolean cursorInHZone = false;
            if (mousePos.x >= windowRect.x && mousePos.x <= windowRect.xMax &&
                mousePos.y >= windowRect.yMax - 5 && mousePos.y <= windowRect.yMax)
                cursorInVZone = true;
            if (mousePos.x >= windowRect.xMax - 5 && mousePos.x <= windowRect.xMax &&
                mousePos.y >= windowRect.y && mousePos.y <= windowRect.yMax)
                cursorInHZone = true;
            if (Input.GetMouseButtonDown(0))
            {
                if (!windowLocked && cursorInHZone && cursorInVZone && resizing == CursorType.Default)
                {
                    activeResizeWindow = winName;
                    resizing = CursorType.ResizeNSEW;
                    originalWindow = windowRect;
                    originalMousePosition = Mouse.screenPos;
                    SetCursor(CursorType.ResizeEW);
                }
                else if (!windowLocked && cursorInVZone && resizing == CursorType.Default)
                {
                    activeResizeWindow = winName;
                    resizing = CursorType.ResizeNS;
                    originalWindow = windowRect;
                    originalMousePosition = Mouse.screenPos;
                    SetCursor(CursorType.ResizeNS);
                }
                else if (!windowLocked && cursorInHZone && resizing == CursorType.Default)
                {
                    activeResizeWindow = winName;
                    resizing = CursorType.ResizeEW;
                    originalWindow = windowRect;
                    originalMousePosition = Mouse.screenPos;
                    SetCursor(CursorType.ResizeEW);
                }
            }
            else if (Input.GetMouseButtonUp(0) && resizing != CursorType.Default)
            {
                activeResizeWindow = "";
                resizing = CursorType.Default;
                SetCursor(CursorType.Default);
            }

            if (activeResizeWindow == "")
            {
                if (cursorInHZone && cursorInVZone && !windowLocked) // Set cursor to ResizeNS if we're hovering over the bottom edge of the window
                    SetCursor(CursorType.ResizeNSEW);
                else if (cursorInVZone && !cursorInHZone && !windowLocked) // Set cursor to ResizeNS if we're hovering over the bottom edge of the window
                    SetCursor(CursorType.ResizeNS);
                else if (cursorInHZone && !cursorInVZone && !windowLocked) // Set cursor to ResizeNS if we're hovering over the bottom edge of the window
                    SetCursor(CursorType.ResizeEW);

                else if (!cursorInVZone && !cursorInHZone && resizing == CursorType.Default)
                    SetCursor(CursorType.Default);
            }
            if (activeResizeWindow == winName)
            {

                if ((resizing == CursorType.ResizeNS || resizing == CursorType.ResizeNSEW) && windowRect.height >= minimumHeight)
                    windowRect.height = originalWindow.height - (originalMousePosition.y - Mouse.screenPos.y);

                if (windowRect.height < minimumHeight)
                    windowRect.height = minimumHeight;

                if ((resizing == CursorType.ResizeEW || resizing == CursorType.ResizeNSEW) && windowRect.width >= minimumWidth)
                    windowRect.width = originalWindow.width - (originalMousePosition.x - Mouse.screenPos.x);

                if (windowRect.width < minimumWidth)
                    windowRect.width = minimumWidth;
            }
        }

        void ResizerUpdate()
        {
            if (HighLogic.CurrentGame == null)
                return;

            // Following is the Window resize code
            if (buttonTextures.Count() == 0)
            {
                buttonTextures.Add("CursorResizeNS", GameDatabase.Instance.GetTexture(modDir + "/Icons/CursorResizeNS", false));
                buttonTextures.Add("CursorResizeEW", GameDatabase.Instance.GetTexture(modDir + "/Icons/CursorResizeEW", false));
                buttonTextures.Add("CursorResizeNSEW", GameDatabase.Instance.GetTexture(modDir + "/Icons/CursorResizeNSEW", false));
            }

            updated = false;

            CheckForResize("ExceptionWindow", ref position);
        }



        /// <summary>
        /// Sets the cursor texture
        /// </summary>
        /// <param name="type"></param>
        public void SetCursor(CursorType type)
        {
            if (type != CursorType.Default)
                updated = true;
            if (type == CursorType.ResizeNS)
                Cursor.SetCursor((Texture2D)buttonTextures["CursorResizeNS"], new Vector2(11, 11), CursorMode.ForceSoftware);
            else
            if (type == CursorType.ResizeEW)
                Cursor.SetCursor((Texture2D)buttonTextures["CursorResizeEW"], new Vector2(11, 11), CursorMode.ForceSoftware);
            else
            if (type == CursorType.ResizeNSEW)
                Cursor.SetCursor((Texture2D)buttonTextures["CursorResizeNSEW"], new Vector2(11, 11), CursorMode.ForceSoftware);
            else if (type == CursorType.Default)
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        #endregion
    }
}
