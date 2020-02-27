/*
Copyright 2019 David Hancock

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
using System.Windows.Forms; 
using System.Diagnostics;
using System.Security.Permissions;
using System.Diagnostics.CodeAnalysis;

using KeePass.Plugins;
using KeePass.Resources;
using KeePass.Util;
using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Security;
using KeePass.App;
using KeePassLib.Utility;
using System.Globalization;

namespace FieldChooser
{
    public partial class FieldChooserForm : Form
    {
        private IPluginHost Host { get; set; }
        private ProtectedStringDictionary Fields { get; set; }
        private readonly List<CharacterSelectorRow> characterSelectorRows = new List<CharacterSelectorRow>();
        private int previousFieldIndex = int.MinValue;


        private FieldChooserForm()
        {
            InitializeComponent();
        }


        public FieldChooserForm(IPluginHost host, ProtectedStringDictionary fields) : this()
        {
            Debug.Assert(host != null);
            Debug.Assert(fields != null);

            Host = host;
            Fields = fields;
        }


        private void FieldChooserForm_Load(object sender, EventArgs e)
        {
            // the standard KeePass app icon
            Icon = AppIcons.Default;

            // link a index combo box with its associated text box
            characterSelectorRows.Add(new CharacterSelectorRow(indexComboBox1, charTextBox1));
            characterSelectorRows.Add(new CharacterSelectorRow(indexComboBox2, charTextBox2));
            characterSelectorRows.Add(new CharacterSelectorRow(indexComboBox3, charTextBox3));
            characterSelectorRows.Add(new CharacterSelectorRow(indexComboBox4, charTextBox4));

            // build the field combo box items
            List<FieldEntry> standardFields = new List<FieldEntry>();
            List<FieldEntry> userFields = new List<FieldEntry>();

            foreach (KeyValuePair<string, ProtectedString> kvp in Fields)
            {
                if (FieldChooserExt.FieldIsValid(kvp))
                {
                    if (kvp.Key == PwDefs.PasswordField)
                        standardFields.Insert(0, new FieldEntry(KPRes.Password, kvp.Value)); 
                    else if (kvp.Key == PwDefs.UserNameField)
                        standardFields.Add(new FieldEntry(KPRes.UserName, kvp.Value)); 
                    else
                        userFields.Add(new FieldEntry(kvp.Key, kvp.Value)); 
                }
            }

            // the plugin's menu item shouldn't have been enabled
            Debug.Assert((standardFields.Count > 0) || (userFields.Count > 0));

            // the user fields are stored by KeePass in a sorted dictionary
            if (userFields.Count > 0)
                standardFields.AddRange(userFields);

            fieldComboBox.DataSource = standardFields;
            fieldComboBox.DropDownWidth = Utils.CalculateDropDownWidth(fieldComboBox);
        }


        private void FieldComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Debug.Assert(fieldComboBox.SelectedItem is FieldEntry);

            // ensure that the index has changed
            if (fieldComboBox.SelectedIndex != previousFieldIndex)
            {
                previousFieldIndex = fieldComboBox.SelectedIndex;

                ProtectedString pString = (fieldComboBox.SelectedItem as FieldEntry).Value;

                // adjust the number of combo box items
                int requiredComboBoxItemCount = pString.Length + 1;

                if (indexComboBox1.Items.Count != requiredComboBoxItemCount)
                { 
                    if (indexComboBox1.Items.Count < requiredComboBoxItemCount)
                    {
                        for (int index = indexComboBox1.Items.Count; index < requiredComboBoxItemCount; index++)
                        {
                            string value = index.ToString(CultureInfo.CurrentCulture);

                            foreach (CharacterSelectorRow row in characterSelectorRows)
                                row.IndexComboBox.Items.Add(value);
                        }
                    }
                    else
                    {
                        for (int index = indexComboBox1.Items.Count - 1; index >= requiredComboBoxItemCount; index--)
                        {
                            foreach (CharacterSelectorRow row in characterSelectorRows)
                                row.IndexComboBox.Items.RemoveAt(index);
                        }
                    }
                }

                // reset the remaining ui state
                protectButton.Enabled = false;

                foreach (CharacterSelectorRow row in characterSelectorRows)
                {
                    if (row.IndexComboBox.SelectedIndex != 0)
                    {
                        row.IndexComboBox.SelectedIndexChanged -= IndexComboBox_SelectedIndexChanged;
                        row.IndexComboBox.SelectedIndex = 0;
                        row.IndexComboBox.SelectedIndexChanged += IndexComboBox_SelectedIndexChanged;
                    }

                    row.CharTextBox.Text = string.Empty;
                    row.CharTextBox.UseSystemPasswordChar = pString.IsProtected;
                }
            }
        }



        private void IndexComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Debug.Assert(sender is ComboBox);

            bool entryProtected = false;
            bool characterSelected = false;

            foreach (CharacterSelectorRow row in characterSelectorRows)
            {
                if (ReferenceEquals(row.IndexComboBox, sender))
                    entryProtected = LoadCharacterTextBox(row.IndexComboBox.SelectedIndex, row.CharTextBox);
                
                characterSelected |= row.IndexComboBox.SelectedIndex > 0 ;
            }

            protectButton.Enabled = entryProtected && characterSelected;
        }



        private bool LoadCharacterTextBox(int selectedIndex, TextBox textBox)
        {
            Debug.Assert(fieldComboBox.SelectedItem is FieldEntry);

            ProtectedString pString = (fieldComboBox.SelectedItem as FieldEntry).Value;

            Debug.Assert(selectedIndex <= pString.Length);

            if (selectedIndex < 1)
                textBox.Text = string.Empty;
            else
            {
                char[] chars = pString.ReadChars();

                textBox.Text = chars[selectedIndex - 1].ToString(CultureInfo.CurrentCulture);

                if (pString.IsProtected)
                    MemUtil.ZeroArray(chars);
            }

            return pString.IsProtected;
        }



        // avoids warnings CA2122 and CA2133
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if ((keyData == Keys.Escape) || (keyData == Keys.Enter))
            {
                Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
        
         

        private void CopyButton_Click(object sender, EventArgs e)
        {
            Debug.Assert(fieldComboBox.SelectedItem is FieldEntry);

            ProtectedString pString = (fieldComboBox.SelectedItem as FieldEntry).Value;

            if (ClipboardUtil.CopyAndMinimize(pString, false, this, null, null))
                Host.MainWindow.StartClipboardCountdown();
        }



        private void ProtectButton_Click(object sender, EventArgs e)
        {
            foreach (CharacterSelectorRow row in characterSelectorRows)
                row.CharTextBox.UseSystemPasswordChar = !row.CharTextBox.UseSystemPasswordChar;
        }



        private void ProtectButtonEnabledStateChanged(object sender, EventArgs e)
        {
            if (protectButton.Enabled)
                protectButton.Image = Properties.Resources.Dots;
            else
                protectButton.Image = Properties.Resources.DotsDisabled;
        }


        private void CharTextBox_TextChanged(object sender, EventArgs e)
        {
            Debug.Assert(sender is TextBox);

            TextBox tb = sender as TextBox;
            tb.Enabled = !string.IsNullOrEmpty(tb.Text);
        }




        private sealed class CharacterSelectorRow
        {
            public ComboBox IndexComboBox { get; private set; }
            public TextBox CharTextBox { get; private set; }

            public CharacterSelectorRow(ComboBox comboBox, TextBox textBox)
            {
                Debug.Assert(comboBox != null);
                Debug.Assert(textBox != null);

                IndexComboBox = comboBox;
                CharTextBox = textBox;
            }
        }


        private sealed class FieldEntry
        {
            // display name of the field
            private string Name { get; set; }
            // the field data
            public ProtectedString Value { get; private set; }


            public FieldEntry(string name, ProtectedString value)
            {
                Debug.Assert(!string.IsNullOrEmpty(name));
                Debug.Assert(value != null);

                Name = name;
                Value = value;
            }


            public override string ToString()
            {
                return Name;
            }
        }

        private static class Utils
        {
            public static int CalculateDropDownWidth(ComboBox comboBox)
            {
                Debug.Assert(comboBox != null);

                int width = comboBox.Width;

                foreach (object obj in comboBox.Items)
                {
                    string text = comboBox.GetItemText(obj);
                    int itemWidth = TextRenderer.MeasureText(text, comboBox.Font).Width;

                    if (itemWidth > width)
                        width = itemWidth;
                }

                return width;
            }
        }
    }
}
