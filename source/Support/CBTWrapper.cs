using System;
using System.Reflection;
using UnityEngine;

namespace ExceptionDetector.Support
{
    internal static class CBTWrapper
    {
        private static bool initialized;
        private static bool available;

        private static MethodInfo guiLayoutWindowMethod;
        private static MethodInfo guiLayoutWindowSkinMethod;

        private static void Initialize()
        {
            if (initialized)
                return;

            initialized = true;

            string[] typeNames =
            {
                "ClickThroughFix.ClickThruBlocker, ClickThroughBlocker",
                "ClickThroughBlocker.ClickThruBlocker, ClickThroughBlocker",
                "ClickThruBlocker, ClickThroughBlocker"
            };

            Type blockerType = null;

            foreach (string typeName in typeNames)
            {
                blockerType = Type.GetType(typeName, false);

                if (blockerType != null)
                    break;
            }

            if (blockerType == null)
                return;

            guiLayoutWindowMethod = blockerType.GetMethod(
                "GUILayoutWindow",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new Type[]
                {
                    typeof(int),
                    typeof(Rect),
                    typeof(GUI.WindowFunction),
                    typeof(string),
                    typeof(GUILayoutOption[])
                },
                null
            );

            guiLayoutWindowSkinMethod = blockerType.GetMethod(
                "GUILayoutWindow",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new Type[]
                {
                                typeof(int),
                                typeof(Rect),
                                typeof(GUI.WindowFunction),
                                typeof(string),
                                typeof(GUISkin),
                                typeof(GUILayoutOption[])
                },
                null
            );

            available = guiLayoutWindowMethod != null;
        }

        public static Rect GUILayoutWindow(
            int id,
            Rect screenRect,
            GUI.WindowFunction func,
            string text,
            params GUILayoutOption[] options
        )
        {
            return GUILayoutWindow(id, screenRect,func, text, GUI.skin, options);
        }
        public static Rect GUILayoutWindow(
            int id,
            Rect screenRect,
            GUI.WindowFunction func,
            string text,
            GUISkin skin,
            params GUILayoutOption[] options
        )
        {
            Initialize();

            // If ClickThroughBlocker exists, use it
            if (available)
            {
                try
                {
                    object result = guiLayoutWindowMethod.Invoke(
                        null,
                        new object[]
                        {
                            id,
                            screenRect,
                            func,
                            text,
                            options
                        }
                    );

                    if (result is Rect)
                        return (Rect)result;
                }
                catch (Exception ex)
                {
                    Debug.LogError("[CBTWrapper] Failed calling ClickThruBlocker.GUILayoutWindow: " + ex);
                }
            }

            // Fallback to normal GUILayout.Window
            return GUILayout.Window(
                id,
                screenRect,
                func,
                text,
                options
            );
        }
    }
}