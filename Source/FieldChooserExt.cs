/*
Copyright 2019-2020 David Hancock

This file is part of the FieldChooser plugin for KeePass 2.

FieldChooser is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 2 of the License, or
(at your option) any later version.

FieldChooser is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with FieldChooser.  If not, see<https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

using KeePass.Plugins;
using KeePass.Resources;
using KeePassLib;
using KeePassLib.Security;

namespace FieldChooser
{

    public sealed class FieldChooserExt : Plugin
    {
        private IPluginHost Host { get; set; }
        private readonly List<ToolStripMenuItem> MenuItems = new List<ToolStripMenuItem>();

        /// <summary>
        /// The <c>Initialize</c> method is called by KeePass when
        /// you should initialize your plug in.
        /// </summary>
        /// <param name="host">Plug in host interface. Through this
        /// interface you can access the KeePass main window, the
        /// currently opened database, etc.</param>
        /// <returns>You must return <c>true</c> in order to signal
        /// successful initialization. If you return <c>false</c>,
        /// KeePass unloads your plug in (without calling the
        /// <c>Terminate</c> method of your plug in).</returns>
        public override bool Initialize(IPluginHost host)
        {
            if (host == null)
                return false;

            Host = host;
            Host.MainWindow.UIStateUpdated += MainWindow_UIStateUpdated;
            return true;           
        }


        private void MainWindow_UIStateUpdated(object sender, EventArgs e)
        {
            if (MenuItems.Count > 0)
            {
                bool enable = false;

                if (Host.MainWindow.GetSelectedEntriesCount() == 1)
                {
                    // only enable if one of the selected entry's relevant fields has some data
                    PwEntry entry = Host.MainWindow.GetSelectedEntry(true);

                    foreach (KeyValuePair<string, ProtectedString> kvp in entry.Strings)
                    {
                        if (FieldIsValid(kvp))
                        {
                            enable = true;
                            break;
                        }
                    }
                }

                foreach (ToolStripMenuItem menuItem in MenuItems)
                    menuItem.Enabled = enable;
            }
        }


        private static bool FieldIsValid(KeyValuePair<string, ProtectedString> field)
        {
            Debug.Assert(field.Value != null);
            Debug.Assert(field.Key != null);

            if (field.Value.IsEmpty)
                return false;

            if ((field.Key == PwDefs.NotesField) || (field.Key == PwDefs.UrlField) || (field.Key == PwDefs.TitleField))
                return false;

            return true;
        }



        /// <summary>
        /// Get a menu item of the plugin. See
        /// https://keepass.info/help/v2_dev/plg_index.html#co_menuitem
        /// </summary>
        /// <param name="t">Type of the menu that the plugin should
        /// return an item for.</param>
        public override ToolStripMenuItem GetMenuItem(PluginMenuType t)
        {
            if (t != PluginMenuType.Entry)
                return null;

            // In KeePass 2.41 this is called once and the single menu item added to an
            // entry's context menu. In KeePass 2.42.1 it is called twice, the menus are
            // added to the context menu and the new Entry drop down menu on the main window.
            ToolStripMenuItem menuItem = new ToolStripMenuItem(Properties.Resources.menu_text, null, new ToolStripMenuItem());


            menuItem.DropDownOpening += delegate (object sender, EventArgs e)
            {
                // dump existing...
                menuItem.DropDownItems.Clear();
               
                // rebuild current...
                List<ToolStripMenuItem> userFields = new List<ToolStripMenuItem>();
                
                PwEntry entry = Host.MainWindow.GetSelectedEntry(true);

                foreach (KeyValuePair<string, ProtectedString> kvp in entry.Strings)
                {
                    if (FieldIsValid(kvp))
                    {
                        ToolStripMenuItem subMenuItem = new ToolStripMenuItem
                        {
                            Image = Properties.Resources.Menu_16x,
                            Tag = kvp.Value
                        };

                        if (kvp.Key == PwDefs.PasswordField)
                        {
                            subMenuItem.Text = KPRes.Password;
                            menuItem.DropDownItems.Insert(0, subMenuItem);
                        }
                        else if (kvp.Key == PwDefs.UserNameField)
                        {
                            subMenuItem.Text = KPRes.UserName;
                            menuItem.DropDownItems.Add(subMenuItem);
                        }
                        else
                        {
                            subMenuItem.Text = kvp.Key;
                            userFields.Add(subMenuItem);
                        }
                    }
                }

                // the plugin's menu item shouldn't have been enabled
                Debug.Assert((menuItem.DropDownItems.Count > 0) || (userFields.Count > 0));

                if (userFields.Count > 0)
                    menuItem.DropDownItems.AddRange(userFields.ToArray());
            };


            menuItem.DropDownItemClicked += delegate (object sender, ToolStripItemClickedEventArgs e)
            {
                using (FieldChooserForm form = new FieldChooserForm(Host, menuItem.DropDownItems, e.ClickedItem))
                {
                    form.ShowDialog(Host.MainWindow);
                }
            };


            // record each menu item created
            MenuItems.Add(menuItem);

            return menuItem;
        }



        /// <summary>
        /// URL of a version information file. See
        /// https://keepass.info/help/v2_dev/plg_index.html#upd
        /// </summary>
        public override string UpdateUrl
        {
            get
            {
#if DEBUG
                string dir = System.IO.Path.GetDirectoryName(typeof(FieldChooserExt).Assembly.Location);
                return System.IO.Path.Combine(dir, "FieldChooser.version");
#else
                return "https://raw.githubusercontent.com/DHancock/FieldChooser/master/FieldChooser.version";
#endif
            }
        }



        /// <summary>
        /// Get a handle to a 16x16 icon representing the plugin.
        /// This icon is shown in the plugin management window of
        /// KeePass for example.
        /// </summary>
        public override Image SmallIcon
        {
            get { return Properties.Resources.Menu_16x; }
        }


        /// <summary>
        /// The <c>Terminate</c> function is called by KeePass when
        /// you should free all resources, close open files/streams,
        /// etc. It is also recommended that you remove all your
        /// plugin menu items from the KeePass menu.
        /// </summary>
        public override void Terminate()
        {
            Host.MainWindow.UIStateUpdated -= MainWindow_UIStateUpdated;

            foreach (ToolStripMenuItem menuItem in MenuItems)
            {                
                ToolStrip parent = menuItem.GetCurrentParent();

                if (parent != null)
                    parent.Items.Remove(menuItem);

                menuItem.Dispose();
            }

            MenuItems.Clear();
        }
    }
}