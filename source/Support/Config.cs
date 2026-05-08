using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ExceptionDetectorEnhanced
{
    public class Config
    {
        private static string _showFullLog = "ShowFullLog";
        private static string _hideKnowns = "HideKnowns";
        private static string _showInfoMessage = "ShowInfoMessages";
        private static string _singlePass = "SinglePass";
        private static string _doublePass = "DoublePass";
        private static string _whitelist = "Whitelist";
        private static string _alwayslist = "Alwayslist";
        private static string _wordwrap = "WordWrap";
        private static string _usewhitelist = "UseWhitelist";
        private static string _usealwayslist = "UseAlwayslist";
        private static string _usealtskin = "UseAltSkin";
        private static string _showatstartup = "ShowAtStartup";

        private static string _bold = "Bold";

        private static string _x = "X";
        private static string _y = "Y";
        private static string _width = "Width";
        private static string _height = "Height";


        public static void Load()
        {
            try
            {
                if (File.Exists(ExceptionDetectorEnhanced.SettingsFile))
                {

                    var node = ConfigNode.Load(ExceptionDetectorEnhanced.SettingsFile);
                    if (node == null) return;

                    var root = node.GetNode("ExceptionDetectorEnhanced");
                    if (root == null) return;

                    var settings = root.GetNode("Config");
                    if (settings == null) return;

                    var singleNode = settings.GetNode(_singlePass);
                    if (singleNode != null)
                        ConvertToDictionary(singleNode, ExceptionDetectorEnhanced.SinglePassValues);

                    var doubleNode = settings.GetNode(_doublePass);
                    if (doubleNode != null)
                        ConvertToDictionary(doubleNode, ExceptionDetectorEnhanced.DoublePassValues);

                    var whitelistNode = settings.GetNode(_whitelist);
                    if (whitelistNode != null)
                        ConvertToDictionary(whitelistNode, ExceptionDetectorEnhanced.WhitelistValues);

                    var alwayslistNode = settings.GetNode(_alwayslist);
                    if (alwayslistNode != null)
                        ConvertToDictionary(alwayslistNode, ExceptionDetectorEnhanced.AlwayslistValues);

                    var set = settings.GetValue(_showFullLog);
                    if (bool.TryParse(set, out var settf)) ExceptionDetectorEnhanced.FullLog = settf;

                    var knowns = settings.GetValue(_hideKnowns);
                    if (bool.TryParse(knowns, out var knowntf)) ExceptionDetectorEnhanced.HideKnowns = knowntf;

                    var shInfo = settings.GetValue(_showInfoMessage);
                    if (bool.TryParse(shInfo, out var llmtf)) ExceptionDetectorEnhanced.ShowInfoMessage = llmtf;

                    var wordwrap = settings.GetValue(_wordwrap);
                    if (bool.TryParse(wordwrap, out var wrap)) ExceptionDetectorEnhanced.WordWrap = wrap;

                    var x = settings.GetValue(_x);
                    var y = settings.GetValue(_y);
                    var width = settings.GetValue(_width);
                    var height = settings.GetValue(_height);

                    if (float.TryParse(x, out var X)) ExceptionDetectorEnhanced.position.x = X;
                    if (float.TryParse(y, out var Y)) ExceptionDetectorEnhanced.position.y = Y;
                    if (float.TryParse(width, out var Width)) ExceptionDetectorEnhanced.position.width = Width;
                    if (float.TryParse(height, out var Height)) ExceptionDetectorEnhanced.position.height = Height;

                    var bold = settings.GetValue(_bold);
                    if (bool.TryParse(bold, out var fw)) ExceptionDetectorEnhanced.Bold = fw;

                    var usewhitelist = settings.GetValue(_usewhitelist);
                    if (bool.TryParse(usewhitelist, out var uwl)) ExceptionDetectorEnhanced.UseWhitelist = uwl;

                    var usealwayslist = settings.GetValue(_usealwayslist);
                    if (bool.TryParse(usealwayslist, out var ual)) ExceptionDetectorEnhanced.UseAlwayslist = ual;

                    var useAltSkin = settings.GetValue(_usealtskin);
                    if (bool.TryParse(useAltSkin, out var uas)) ExceptionDetectorEnhanced.UseAltSkin = uas;

                    var showAtStartup = settings.GetValue(_showatstartup);
                    if (bool.TryParse(showAtStartup, out var sas)) ExceptionDetectorEnhanced.ShowAtStartup = sas;

                }
                Save();
            }
            catch (Exception ex)
            {
                ExceptionDetectorEnhanced.WriteLog(ex.ToString());
            }
        }

        private static void ConvertToDictionary(ConfigNode node, Dictionary<string, string> passValues)
        {
            for (int x = 0; x < node.CountValues; x++)
            {
                if (!passValues.ContainsKey(node.values[x].name))
                    passValues.Add(node.values[x].name, node.values[x].value);
            }
        }

        private static ConfigNode ConvertFromDictionary(String name, Dictionary<string, string> passValues)
        {
            ConfigNode node = new ConfigNode(name);

            foreach (KeyValuePair<string, string> val in passValues)
            {
                node.AddValue(val.Key, val.Value);
            }
            return node;
        }

        public static void Save()
        {
            try
            {
                var node = new ConfigNode();
                var root = node.AddNode("ExceptionDetectorEnhanced");
                var settings = root.AddNode("Config");
                settings.AddValue(_showFullLog, ExceptionDetectorEnhanced.FullLog);
                settings.AddValue(_hideKnowns, ExceptionDetectorEnhanced.HideKnowns);
                settings.AddValue(_showInfoMessage, ExceptionDetectorEnhanced.ShowInfoMessage);
                var sNode = settings.AddNode(ConvertFromDictionary(_singlePass, ExceptionDetectorEnhanced.SinglePassValues));
                var dNode = settings.AddNode(ConvertFromDictionary(_doublePass, ExceptionDetectorEnhanced.DoublePassValues));

                var wnode = settings.AddNode(ConvertFromDictionary(_whitelist, ExceptionDetectorEnhanced.WhitelistValues));
                var anode = settings.AddNode(ConvertFromDictionary(_alwayslist, ExceptionDetectorEnhanced.AlwayslistValues));

                settings.AddValue(_wordwrap, ExceptionDetectorEnhanced.WordWrap);
                settings.AddValue(_bold, ExceptionDetectorEnhanced.Bold);
                settings.AddValue(_usewhitelist, ExceptionDetectorEnhanced.UseWhitelist);
                settings.AddValue(_usealwayslist, ExceptionDetectorEnhanced.UseAlwayslist);
                settings.AddValue(_usealtskin, ExceptionDetectorEnhanced.UseAltSkin);
                settings.AddValue(_showatstartup, ExceptionDetectorEnhanced.ShowAtStartup);

                settings.AddValue(_x, ExceptionDetectorEnhanced.fiGui.position.x);
                settings.AddValue(_y, ExceptionDetectorEnhanced.fiGui.position.y);
                settings.AddValue(_width, ExceptionDetectorEnhanced.fiGui.position.width);
                settings.AddValue(_height, ExceptionDetectorEnhanced.fiGui.position.height);

                node.Save(ExceptionDetectorEnhanced.SettingsFile);
            }
            catch (Exception ex)
            {
                ExceptionDetectorEnhanced.WriteLog(ex.ToString());
            }
        }
    }
}
