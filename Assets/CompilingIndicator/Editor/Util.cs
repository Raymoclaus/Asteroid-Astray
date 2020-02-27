using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace UsingTheirs.CompilingIndicator
{
    public static class Util
    {
        internal static Type[] GetAllDerivedTypes(this AppDomain aAppDomain, Type aType)
        {
#if UNITY_2019_2_OR_NEWER
            return TypeCache.GetTypesDerivedFrom(aType).ToArray();
#else
            var result = new List<Type>();
            var assemblies = aAppDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetLoadableTypes();
                foreach (var type in types)
                {
                    if (type.IsSubclassOf(aType))
                        result.Add(type);
                }
            }
            return result.ToArray();
#endif
        }

        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }

        static UnityEngine.Object s_MainWindow = null;

        internal static Rect GetEditorMainWindowPos()
        {
            if (s_MainWindow == null)
            {
                var containerWinType = AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(ScriptableObject))
                    .FirstOrDefault(t => t.Name == "ContainerWindow");
                if (containerWinType == null)
                    throw new MissingMemberException(
                        "Can't find internal type ContainerWindow. Maybe something has changed inside Unity");
                var showModeField =
                    containerWinType.GetField("m_ShowMode", BindingFlags.NonPublic | BindingFlags.Instance);
                if (showModeField == null)
                    throw new MissingFieldException(
                        "Can't find internal fields 'm_ShowMode'. Maybe something has changed inside Unity");
                var windows = Resources.FindObjectsOfTypeAll(containerWinType);
                foreach (var win in windows)
                {
                    var showMode = (int) showModeField.GetValue(win);
                    if (showMode == 4) // main window
                    {
                        s_MainWindow = win;
                        break;
                    }
                }
            }

            if (s_MainWindow == null)
                return new Rect(0, 0, 800, 600);

            var positionProperty =
                s_MainWindow.GetType().GetProperty("position", BindingFlags.Public | BindingFlags.Instance);
            if (positionProperty == null)
                throw new MissingFieldException(
                    "Can't find internal fields 'position'. Maybe something has changed inside Unity.");
            return (Rect) positionProperty.GetValue(s_MainWindow, null);
        }

        internal static Rect GetCenteredWindowPosition(Rect parentWindowPosition, Vector2 size)
        {
            var pos = new Rect
            {
                x = 0, y = 0,
                width = Mathf.Min(size.x, parentWindowPosition.width * 0.90f),
                height = Mathf.Min(size.y, parentWindowPosition.height * 0.90f)
            };
            var w = (parentWindowPosition.width - pos.width) * 0.5f;
            var h = (parentWindowPosition.height - pos.height) * 0.5f;
            pos.x = parentWindowPosition.x + w;
            pos.y = parentWindowPosition.y + h;
            return pos;
        }

        internal static Rect GetBottomRightPosition(Rect parentWindowPosition, Vector2 size, Vector2 margin)
        {
            var pos = new Rect
            {
                x = 0, y = 0,
                width = Mathf.Min(size.x, parentWindowPosition.width * 0.90f),
                height = Mathf.Min(size.y, parentWindowPosition.height * 0.90f)
            };
            var w = (parentWindowPosition.width - pos.width - margin.x);
            var h = (parentWindowPosition.height - pos.height - margin.y);
            pos.x = parentWindowPosition.x + w;
            pos.y = parentWindowPosition.y + h;
            return pos;
        }

        internal static IEnumerable<MethodInfo> GetAllMethodsWithAttribute<T>(
            BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
        {
            Assembly assembly = typeof(Selection).Assembly;
            var managerType = assembly.GetTypes().First(t => t.Name == "EditorAssemblies");
            var method = managerType.GetMethod("Internal_GetAllMethodsWithAttribute",
                BindingFlags.NonPublic | BindingFlags.Static);
            var arguments = new object[] {typeof(T), bindingFlags};
//        return ((method.Invoke(null, arguments) as object[]) ?? throw new InvalidOperationException())
//            .Cast<MethodInfo>();

            var res = method.Invoke(null, arguments) as object[];
            if (res == null) throw new InvalidOperationException();
            return res.Cast<MethodInfo>();
        }

        internal static Rect GetMainWindowCenteredPosition(Vector2 size)
        {
            var mainWindowRect = GetEditorMainWindowPos();
            return GetCenteredWindowPosition(mainWindowRect, size);
        }

        internal static Rect GetMainWindowBottomRightPosition(Vector2 size)
        {
            var mainWindowRect = GetEditorMainWindowPos();
            return GetBottomRightPosition(mainWindowRect, size, new Vector2(20, 20));
        }

        internal static void ShowDropDown(this EditorWindow window, Vector2 size, bool center)
        {
            window.maxSize = window.minSize = size;
            if (center)
                window.position = GetMainWindowCenteredPosition(size);
            else
                window.position = GetMainWindowBottomRightPosition(size);

            window.ShowPopup();

            Assembly assembly = typeof(EditorWindow).Assembly;

            var editorWindowType = typeof(EditorWindow);
            var hostViewType = assembly.GetType("UnityEditor.HostView");
            var containerWindowType = assembly.GetType("UnityEditor.ContainerWindow");

            var parentViewField = editorWindowType.GetField("m_Parent", BindingFlags.Instance | BindingFlags.NonPublic);
            var parentViewValue = parentViewField.GetValue(window);

            hostViewType.InvokeMember("AddToAuxWindowList",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, parentViewValue,
                null);

            // Dropdown windows should not be saved to layout
            var containerWindowProperty =
                hostViewType.GetProperty("window", BindingFlags.Instance | BindingFlags.Public);
            var parentContainerWindowValue = containerWindowProperty.GetValue(parentViewValue, null);
            var dontSaveToLayoutField =
                containerWindowType.GetField("m_DontSaveToLayout", BindingFlags.Instance | BindingFlags.NonPublic);
            dontSaveToLayoutField.SetValue(parentContainerWindowValue, true);
            Debug.Assert((bool) dontSaveToLayoutField.GetValue(parentContainerWindowValue));
        }
    }
}