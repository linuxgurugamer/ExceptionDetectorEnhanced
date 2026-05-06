using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ExceptionDetector
{
    public class Config
    {
        private static string _showFullLog = "ShowFullLog";
        private static string _hideKnowns = "HideKnowns";
        private static string _showInfoMessage = "ShowInfoMessages";
        private static string _singlePass = "SinglePass";
        private static string _doublePass = "DoublePass";
        private static string _whitelist = "Whitelist";
        private static string _wordwrap = "WordWrap";
        //private static string _fixedwidth = "FixedWidth";
        private static string _usewhitelist = "UseWhitelist";
        private static string _bold = "Bold";

        private static string _x = "X";
        private static string _y = "Y";
        private static string _width = "Width";
        private static string _height = "Height";


        public static void Load()
        {
            try
            {
                if (File.Exists(ExceptionDetector.SettingsFile))
                {

                    var node = ConfigNode.Load(ExceptionDetector.SettingsFile);
                    if (node == null) return;

                    var root = node.GetNode("ExceptionDetector");
                    if (root == null) return;

                    var settings = root.GetNode("Config");
                    if (settings == null) return;

                    var singleNode = settings.GetNode(_singlePass);
                    if (singleNode != null)
                        ConvertToDictionary(singleNode, ExceptionDetector.SinglePassValues);

                    var doubleNode = settings.GetNode(_doublePass);
                    if (doubleNode != null)
                        ConvertToDictionary(doubleNode, ExceptionDetector.DoublePassValues);

                    var whitelistNode = settings.GetNode(_whitelist);
                    if (whitelistNode != null)
                        ConvertToDictionary(whitelistNode, ExceptionDetector.WhitelistValues);

                    var set = settings.GetValue(_showFullLog);
                    if (bool.TryParse(set, out var settf)) ExceptionDetector.FullLog = settf;

                    var knowns = settings.GetValue(_hideKnowns);
                    if (bool.TryParse(knowns, out var knowntf)) ExceptionDetector.HideKnowns = knowntf;

                    var shInfo = settings.GetValue(_showInfoMessage);
                    if (bool.TryParse(shInfo, out var llmtf)) ExceptionDetector.ShowInfoMessage = llmtf;

                    var wordwrap = settings.GetValue(_wordwrap);
                    if (bool.TryParse(wordwrap, out var wrap)) ExceptionDetector.WordWrap = wrap;

                    var x = settings.GetValue(_x);
                    var y = settings.GetValue(_y);
                    var width = settings.GetValue(_width);
                    var height = settings.GetValue(_height);

                    if (float.TryParse(x, out var X)) ExceptionDetector.position.x = X;
                    if (float.TryParse(y, out var Y)) ExceptionDetector.position.y = Y;
                    if (float.TryParse(width, out var Width)) ExceptionDetector.position.width = Width;
                    if (float.TryParse(height, out var Height)) ExceptionDetector.position.height = Height;
#if false
					var fixedwidth = settings.GetValue(_fixedwidth);
                    if (bool.TryParse(fixedwidth, out var fw)) ExceptionDetector.Bold = fw;
#endif
                    var bold = settings.GetValue(_bold);
                    if (bool.TryParse(bold, out var fw)) ExceptionDetector.Bold = fw;

                    var usewhitelist = settings.GetValue(_usewhitelist);
                    if (bool.TryParse(usewhitelist, out var uwl)) ExceptionDetector.UseWhitelist = uwl;

                }
                Save();
            }
            catch (Exception ex)
            {
                ExceptionDetector.WriteLog(ex.ToString());
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
                var root = node.AddNode("ExceptionDetector");
                var settings = root.AddNode("Config");
                settings.AddValue(_showFullLog, ExceptionDetector.FullLog);
                settings.AddValue(_hideKnowns, ExceptionDetector.HideKnowns);
                settings.AddValue(_showInfoMessage, ExceptionDetector.ShowInfoMessage);
                var sNode = settings.AddNode(ConvertFromDictionary(_singlePass, ExceptionDetector.SinglePassValues));
                var dNode = settings.AddNode(ConvertFromDictionary(_doublePass, ExceptionDetector.DoublePassValues));

                settings.AddValue(_wordwrap, ExceptionDetector.WordWrap);
                settings.AddValue(_bold, ExceptionDetector.Bold);
                settings.AddValue(_usewhitelist, ExceptionDetector.UseWhitelist);

                settings.AddValue(_x, ExceptionDetector.fiGui.position.x);
                settings.AddValue(_y, ExceptionDetector.fiGui.position.y);
                settings.AddValue(_width, ExceptionDetector.fiGui.position.width);
                settings.AddValue(_height, ExceptionDetector.fiGui.position.height);

                node.Save(ExceptionDetector.SettingsFile);
            }
            catch (Exception ex)
            {
                ExceptionDetector.WriteLog(ex.ToString());
            }
        }
    }
}
