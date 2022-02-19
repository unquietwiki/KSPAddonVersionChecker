﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KSP_AVC
{
    class CompatibilityOverrideHelpGui : MonoBehaviour
    {
        #region Fields

        private GUIStyle boxStyle;
        private GUIStyle buttonStyle;
        private bool hasCentred;
        private GUIStyle labelStyle;
        private Rect position = new Rect(Screen.width, Screen.height, 640, 800);
        private GUIStyle topLevelTitleStyle;

        #endregion

        #region Methods: protected

        protected void Awake()
        {
            try
            {
                DontDestroyOnLoad(this);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
            Logger.Log("Awake CompatibilityOverrideHelpGui.");
        }

        protected void Start()
        {
            try
            {
                this.InitialiseStyles();
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
        }

        protected void OnDestroy()
        {
            if (Configuration.CfgUpdated)
            {
                Configuration.SaveCfg();
            }
            Logger.Log("Destroyed CompatibilityOverrideHelpGui.");
        }

        protected void OnGUI()
        {
            try
            {
                this.position = GUILayout.Window(this.GetInstanceID(), this.position, this.Window, "KSP Add-on Version Checker - Help", HighLogic.Skin.window);
                this.CentreWindow();
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
        }

        #endregion

        #region HelpWindow
        Vector2 scrollPos;

        private void DrawHelpWindow()
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            GUILayout.BeginVertical();
            GUILayout.Label("Compatibility Override", this.topLevelTitleStyle);
            GUILayout.BeginHorizontal(this.boxStyle, GUILayout.Width(600));
            DrawHelpGeneral();
            GUILayout.EndHorizontal();
            GUILayout.Label("How to use the Override", this.topLevelTitleStyle);
            GUILayout.BeginHorizontal(this.boxStyle, GUILayout.Width(600));
            DrawHelpOverride();
            GUILayout.EndHorizontal();
            GUILayout.Label("Always Compatible", this.topLevelTitleStyle);
            GUILayout.BeginHorizontal(this.boxStyle, GUILayout.Width(600));
            DrawHelpAlwaysOverride();
            GUILayout.EndHorizontal();
            GUILayout.Label("Compat. Version Override", this.topLevelTitleStyle);
            GUILayout.BeginHorizontal(this.boxStyle, GUILayout.Width(600));
            DrawHelpVersionOverride();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        private void DrawHelpGeneral()
        {
            GUILayout.Label("The Compatibility Override provides you some control over the AVC Add-on Version Checker." +
                "\nIt allows you to set an addon to be compatible with a different game version than the one written in the .version file.", this.labelStyle);
        }

        private void DrawHelpAlwaysOverride()
        {
            GUILayout.Label("The left panel, lists addons which will bypass the compatibility check at all. If a name is on this list," +
                "\n\nAVC will no longer report any compatibility issues with this addon." +
                "\n\nYou can add mod names by clicking the  \u25C0  button in the center panel, and remove mod names by clicking the red X", this.labelStyle);
        }

        private void DrawHelpOverride()
        {
            GUILayout.Label(
                "The main window has three panels.  The center panel is the most important to understand.\n" +

                "The center panel shows a list of all addons which are currently reported as incompatible with the running game version," +
                " and the max. game version which is allowed by the .version file. " +
                "\n\nThe addon names are color-coded:" +
                "\n     Yellow = incompatible" +
                "\n     Blue    = affected by any compatibility override" +
                "\n\nThe list also contains two buttons:  \u25C0  and  \u25B6" +
                "\nThese buttons allow you to put an addon name on the \"ALWAYS COMPATIBLE\" list or to draft the version number to the \"VERSION OVERRIDE\" list.  " +
                "If these buttons don't appear, the addon creator doesn't allow any compatibility overrides. " +
                "Update notifications are still provided. in any case", this.labelStyle);
        }

        private void DrawHelpVersionOverride()
        {
            GUILayout.Label("This panel provides control over the compatibility between addons and the game. This will affect many addons at once. " +
                "An active version override is displayed like this:" +
                "\n\n     1.4.1 \u279C 1.6.1" +
                "\n\nIn this example, any addon which is compatible with KSP 1.4.1, will become compatible with KSP 1.6.1. " +
                "\n\nThere are different ways to add version numbers:" +
                "\n\n1. If the \u25B6 button is used to add a version number to this list, it will always be set to be compatible with the curent game version. " +
                "\n2. There is a list of toggles listing several prior versions of KSP with a wildcard, clicking the toggle will add that version to the list" +
                "\n3. You can type in one or more version numbers into the text field and click on the \"ADD\" button. Multiple versions need to be separated by a comma and will " +
                "set the first version number to be compatible with every other game version followed." +
                "\n\nA single version number will be handled like the  \u25B6  button. " +
                "\n\nYou can also use a wildcard/asterisk on a single version number, but just on the third (patch) number. " +
                "In order to set each KSP 1.4.x version to be compatible, type in \" 1.4.* \" and click on \"ADD\"." +
                "\n\nClick the red X to remove a line", this.labelStyle);
        }

        #endregion

        #region Methods : Private

        private void Window(int id)
        {
            this.DrawHelpWindow();
            if (GUILayout.Button("CLOSE", this.buttonStyle))
            {
                Destroy(this);
            }
            GUI.DragWindow();
        }

        private void CentreWindow()
        {
            if (this.hasCentred || !(this.position.width > 0) || !(this.position.height > 0))
            {
                return;
            }
            this.position.center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            this.hasCentred = true;
        }

        #endregion

        #region Styles

        private void InitialiseStyles()
        {
            this.boxStyle = new GUIStyle(HighLogic.Skin.box)
            {
                padding = new RectOffset(10, 10, 5, 5),
            };

            this.buttonStyle = new GUIStyle(HighLogic.Skin.button)
            {
                normal =
                {
                    textColor = Color.white
                },
                fontStyle = FontStyle.Bold,
            };

            this.labelStyle = new GUIStyle(HighLogic.Skin.label)
            {
                alignment = TextAnchor.MiddleLeft
            };

            this.topLevelTitleStyle = new GUIStyle(HighLogic.Skin.label)
            {
                normal =
                {
                    textColor = Color.white
                },
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
            };
        }

        #endregion
    }
}
