using KSP.UI.Screens;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

[KSPAddon(KSPAddon.Startup.MainMenu, true)]
public class RegisterToolbar : MonoBehaviour
{
    internal const string MODID = "ExceptionDetector";
    internal const string MODNAME = "ExceptionDetector";

    void Start()
    {
        RegisterToolbarControllerMod(MODID, MODNAME);
    }

    private static void RegisterToolbarControllerMod(string modId, string modName)
    {
        try
        {
            Type tcType = AssemblyLoader.loadedAssemblies
                .Select(a => a.assembly.GetType("ToolbarControl_NS.ToolbarControl"))
                .FirstOrDefault(t => t != null);

            if (tcType == null)
            {
                Debug.Log("[RegisterToolbarControllerMod] ToolbarController not installed, skipping RegisterMod");
                return;
            }

            MethodInfo registerMod = tcType.GetMethod(
                "RegisterMod",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new Type[]
                {
                    typeof(string), // NameSpace
                    typeof(string), // DisplayName
                    typeof(bool),   // useBlizzy
                    typeof(bool),   // useStock
                    typeof(bool)    // NoneAllowed
                },
                null);

            if (registerMod == null)
            {
                Debug.LogWarning("[RegisterToolbarControllerMod] ToolbarControl.RegisterMod method not found");
                return;
            }

            registerMod.Invoke(null, new object[]
            {
                modId,
                modName,
                false, true, true
            });
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[RegisterToolbarControllerMod] ToolbarController RegisterMod failed: " + ex);
        }
    }
}

public class ToolbarWrapper
{
    private readonly MonoBehaviour host;
    private readonly Action onTrue;
    private readonly Action onFalse;

    private Component toolbarControl;
    private ApplicationLauncherButton stockButton;

    public bool UsingToolbarController { get; private set; }

    public ToolbarWrapper(
        MonoBehaviour host,
        Action onTrue,
        Action onFalse,
        ApplicationLauncher.AppScenes scenes,
        string toolbarNamespace,
        string toolbarId,
        string largeToolbarIconActive,
        string largeToolbarIconInactive,
        string smallToolbarIconActive,
        string smallToolbarIconInactive,
        string tooltip)
    {
        this.host = host;
        this.onTrue = onTrue;
        this.onFalse = onFalse;

        if (!TryCreateToolbarController(
                scenes,
                toolbarNamespace,
                toolbarId,
                largeToolbarIconActive,
                largeToolbarIconInactive,
                smallToolbarIconActive,
                smallToolbarIconInactive,
                tooltip))
        {
            CreateStockButton(largeToolbarIconActive, scenes);
        }
    }

    public ToolbarWrapper(
        MonoBehaviour host,
        Action onTrue,
        Action onFalse,
        ApplicationLauncher.AppScenes scenes,
        string toolbarNamespace,
        string toolbarId,
        string largeToolbarIconActive,
        string largeToolbarIconInactive,
        string tooltip)
    {
        this.host = host;
        this.onTrue = onTrue;
        this.onFalse = onFalse;

        if (!TryCreateToolbarController(
                scenes,
                toolbarNamespace,
                toolbarId,
                largeToolbarIconActive,
                largeToolbarIconInactive,
                largeToolbarIconActive,
                largeToolbarIconInactive,
                tooltip))
        {
            CreateStockButton(largeToolbarIconActive, scenes);
        }
    }

    #region ToolbarController (reflection)

    private bool TryCreateToolbarController(
        ApplicationLauncher.AppScenes scenes,
        string toolbarNamespace,
        string toolbarId,
        string largeToolbarIconActive,
        string largeToolbarIconInactive,
        string smallToolbarIconActive,
        string smallToolbarIconInactive,
        string tooltip)
    {
        try
        {
            // Find ToolbarController type
            Type tcType = AssemblyLoader.loadedAssemblies
                .Select(a => a.assembly.GetType("ToolbarControl_NS.ToolbarControl"))
                .FirstOrDefault(t => t != null);

            if (tcType == null)
                return false;

            toolbarControl = host.gameObject.AddComponent(tcType);

            MethodInfo addMethod = FindAddToAllToolbars(tcType);
            if (addMethod == null)
                return false;

            ParameterInfo[] p = addMethod.GetParameters();

            Delegate onTrueDelegate = Delegate.CreateDelegate(
                p[0].ParameterType,
                this,
                nameof(OnTrueProxy));

            Delegate onFalseDelegate = Delegate.CreateDelegate(
                p[1].ParameterType,
                this,
                nameof(OnFalseProxy));

            addMethod.Invoke(toolbarControl, new object[]
            {
                onTrueDelegate,
                onFalseDelegate,
                scenes,
                toolbarNamespace,
                toolbarId,
                largeToolbarIconActive,   // stock icon
                largeToolbarIconInactive,   // toolbar icon
                smallToolbarIconActive,   // stock icon
                smallToolbarIconInactive,   // toolbar icon
                tooltip,
            });

            UsingToolbarController = true;
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[ToolbarWrapper] ToolbarController failed, falling back: " + ex);
            return false;
        }
    }
    internal void SetFalse(bool value = false)
    {
        MethodInfo setFalse = FindSetFalse(toolbarControl.GetType());

        setFalse?.Invoke(toolbarControl, new object[]
        {
            value
        });

    }

    private static MethodInfo FindSetFalse(Type tcType)
    {
        return tcType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .FirstOrDefault(m =>
            {
                if (m.Name != "SetFalse")
                    return false;

                var p = m.GetParameters();

                return p.Length == 1 &&
                       p[0].ParameterType == typeof(bool);
            });
    }

    private static MethodInfo FindAddToAllToolbars(Type tcType)
    {
        return tcType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .FirstOrDefault(m =>
            {
                if (m.Name != "AddToAllToolbars")
                    return false;

                ParameterInfo[] p = m.GetParameters();

                return p.Length == 10 &&
                       typeof(Delegate).IsAssignableFrom(p[0].ParameterType) &&
                       typeof(Delegate).IsAssignableFrom(p[1].ParameterType) &&
                       p[2].ParameterType == typeof(ApplicationLauncher.AppScenes) &&
                       p[3].ParameterType == typeof(string) &&
                       p[4].ParameterType == typeof(string) &&
                       p[5].ParameterType == typeof(string) &&
                       p[6].ParameterType == typeof(string) &&
                       p[7].ParameterType == typeof(string) &&
                       p[8].ParameterType == typeof(string) &&
                       p[9].ParameterType == typeof(string);
            });
    }
    #endregion

    #region Stock Toolbar

    private void CreateStockButton(string iconPath, ApplicationLauncher.AppScenes scenes)
    {
        Texture2D icon = GameDatabase.Instance.GetTexture(iconPath, false);

        stockButton = ApplicationLauncher.Instance.AddModApplication(
            OnTrueProxy,
            OnFalseProxy,
            null,
            null,
            null,
            null,
            scenes,
            icon);

        UsingToolbarController = false;
    }

    #endregion

    #region Proxies

    private void OnTrueProxy()
    {
        onTrue?.Invoke();
    }

    private void OnFalseProxy()
    {
        onFalse?.Invoke();
    }

    #endregion

    #region Public API

    public void SetFalse()
    {
        if (UsingToolbarController && toolbarControl != null)
        {
            MethodInfo m = toolbarControl.GetType().GetMethod("SetFalse");
            m?.Invoke(toolbarControl, null);
        }
        else
        {
            stockButton?.SetFalse(false);
        }
    }

    public void SetTrue()
    {
        if (UsingToolbarController && toolbarControl != null)
        {
            MethodInfo m = toolbarControl.GetType().GetMethod("SetTrue");
            m?.Invoke(toolbarControl, null);
        }
        else
        {
            stockButton?.SetTrue(false);
        }
    }

    public void Destroy()
    {
        if (UsingToolbarController && toolbarControl != null)
        {
            UnityEngine.Object.Destroy(toolbarControl);
            toolbarControl = null;
        }

        if (stockButton != null && ApplicationLauncher.Instance != null)
        {
            ApplicationLauncher.Instance.RemoveModApplication(stockButton);
            stockButton = null;
        }
    }

    #endregion
}