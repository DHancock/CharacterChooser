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
using System.Windows.Forms; 
using System.Diagnostics;
using System.Security.Permissions;
using System.Globalization;
using System.Drawing;
using System.Windows.Forms.VisualStyles;

using KeePass.Plugins;
using KeePass.Resources;
using KeePass.Util;
using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Security;
using KeePass.App;
using KeePassLib.Utility;

using FieldChooser.Properties;


namespace FieldChooser
{
    public partial class FieldChooserForm : Form
    {
        private IPluginHost Host { get; set; }
        private ProtectedStringDictionary Fields { get; set; }
        private readonly List<CharacterSelectorRow> characterSelectorRows = new List<CharacterSelectorRow>();
        private int previousFieldIndex = int.MinValue;

        private const string cWidthKey = "FieldChooser.DialogInfo.Width";
        private const string cXKey = "FieldChooser.DialogInfo.X";
        private const string cYKey = "FieldChooser.DialogInfo.Y";


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

            AdjustLayoutForPasswordFont();

            // the default width and calculated height
            this.MinimumSize = new Size(this.Width, this.Height);

            // only allow width resizing, the height has been calculated
            this.MaximumSize = new Size(int.MaxValue, this.Height);

            // pin the saved width to the default width
            this.Width = Math.Max(this.Width, (int)Host.CustomConfig.GetLong(cWidthKey, 0));

            this.Location = RestoreAndValidateSavedFormLocation();
        }



        private Point RestoreAndValidateSavedFormLocation()
        {
            Point location;

            long savedx = Host.CustomConfig.GetLong(cXKey, long.MinValue);

            if (savedx == long.MinValue)   // no configuration found, center on parent
            {
                location = new Point(this.Owner.Location.X + ((this.Owner.Width - this.Width) / 2),
                                     this.Owner.Location.Y + ((this.Owner.Height - this.Height) / 2));
            }
            else
                location = new Point((int)savedx, (int)Host.CustomConfig.GetLong(cYKey, 0));

            // of the closest screen
            Rectangle workingArea = Screen.FromPoint(location).WorkingArea;

            // It's a dialog, there is no reason for any part of it to be obscured
            // if that's possible. Test and adjust if necessary.

            if ((location.Y + this.Height) > workingArea.Bottom)
                location.Y = workingArea.Bottom - this.Height;

            if (location.Y < workingArea.Top)
                location.Y = workingArea.Top;

            if ((location.X + this.Width) > workingArea.Right)
                location.X = workingArea.Right - this.Width;

            if (location.X < workingArea.Left)
                location.X = workingArea.Left;

            return location;
        }



        private void AdjustLayoutForPasswordFont()
        {
            Font passwordFont = KeePass.Program.Config.UI.PasswordFont.ToFont();

            if (!Utils.FontEquals(passwordFont, charTextBox1.Font))
            {
                int verticalOffset = 0;

                foreach (CharacterSelectorRow row in characterSelectorRows)
                {
                    int oldTextBoxHight = row.CharTextBox.Height;

                    row.CharTextBox.Font = passwordFont;

                    row.CharTextBox.Top += verticalOffset;
                    row.IndexComboBox.Top += verticalOffset;

                    // the text box has a minimum size property so won't shrink
                    verticalOffset += Math.Max(row.CharTextBox.Height - oldTextBoxHight, 0);
                }

                this.Height += verticalOffset;
            }
        }



        private void FieldChooserForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Host.CustomConfig.SetLong(cWidthKey, this.Width);
            Host.CustomConfig.SetLong(cXKey, this.Left);
            Host.CustomConfig.SetLong(cYKey, this.Top);
        }



        // Hijack the hit test message result stopping the OS from showing the vertical 
        // grow cursor. Vertical growing isn't valid for this dialog, it's height is calculated.
        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x0084;

            base.WndProc(ref m);

            if (m.Msg == WM_NCHITTEST)
            {
                HitTestCode result = (HitTestCode)m.Result.ToInt32();

                switch (result)
                {
                    case HitTestCode.TopLeft:
                    case HitTestCode.BottomLeft:
                        m.Result = new IntPtr((int)HitTestCode.Left); break;

                    case HitTestCode.TopRight:
                    case HitTestCode.BottomRight:
                        m.Result = new IntPtr((int)HitTestCode.Right); break;

                    case HitTestCode.Top:
                    case HitTestCode.Bottom:
                        m.Result = new IntPtr((int)HitTestCode.Nowhere); break;
                }
            }
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

                characterSelected |= row.IndexComboBox.SelectedIndex > 0;
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
                protectButton.Image = Resources.Dots;
            else
                protectButton.Image = Resources.DotsDisabled;
        }



        private void CharTextBox_TextChanged(object sender, EventArgs e)
        {
            Debug.Assert(sender is TextBox);

            TextBox tb = sender as TextBox;

            if (string.IsNullOrEmpty(tb.Text))
                tb.Enabled = false;
            else
            {
                tb.Enabled = true;
                toolTip.SetToolTip(tb, GetUnicodeCategoryDescription(tb.Text[0]));
            }
        }



        private void ToolTip_Popup(object sender, PopupEventArgs e)
        {
            Debug.Assert(e.AssociatedControl is TextBox);
            e.Cancel = (e.AssociatedControl as TextBox).UseSystemPasswordChar;
        }



        /// <summary>
        /// Gets a description of the Unicode category that the supplied
        /// character belongs to. Helps the user identify which look a 
        /// like character they are dealing with.
        /// </summary>
        /// <param name="c"></param>
        /// <returns>category description</returns>
        private string GetUnicodeCategoryDescription(char c)
        {
            string s = string.Empty;
            UnicodeCategory uc = Char.GetUnicodeCategory(c);

            switch (uc)
            {
                case UnicodeCategory.UppercaseLetter: s = Resources.Lu; break;
                case UnicodeCategory.LowercaseLetter: s = Resources.Ll; break;
                case UnicodeCategory.TitlecaseLetter: s = Resources.Lt; break;
                case UnicodeCategory.ModifierLetter: s = Resources.Lm; break;
                case UnicodeCategory.OtherLetter: s = Resources.Lo; break;
                case UnicodeCategory.NonSpacingMark: s = Resources.Mn; break;
                case UnicodeCategory.SpacingCombiningMark: s = Resources.Mc; break;
                case UnicodeCategory.EnclosingMark: s = Resources.Me; break;
                case UnicodeCategory.DecimalDigitNumber: s = Resources.Nd; break;
                case UnicodeCategory.LetterNumber: s = Resources.Nl; break;
                case UnicodeCategory.OtherNumber: s = Resources.No; break;
                case UnicodeCategory.SpaceSeparator: s = Resources.Zs; break;
                case UnicodeCategory.LineSeparator: s = Resources.Zl; break;
                case UnicodeCategory.ParagraphSeparator: s = Resources.Zp; break;
                case UnicodeCategory.Control: s = Resources.Cc; break;
                case UnicodeCategory.Format: s = Resources.Cf; break;
                case UnicodeCategory.Surrogate: s = Resources.Cs; break;
                case UnicodeCategory.PrivateUse: s = Resources.Co; break;
                case UnicodeCategory.ConnectorPunctuation: s = Resources.Pc; break;
                case UnicodeCategory.DashPunctuation: s = Resources.Pd; break;
                case UnicodeCategory.OpenPunctuation: s = Resources.Ps; break;
                case UnicodeCategory.ClosePunctuation: s = Resources.Pe; break;
                case UnicodeCategory.InitialQuotePunctuation: s = Resources.Pi; break;
                case UnicodeCategory.FinalQuotePunctuation: s = Resources.Pf; break;
                case UnicodeCategory.OtherPunctuation: s = Resources.Po; break;
                case UnicodeCategory.MathSymbol: s = Resources.Sm; break;
                case UnicodeCategory.CurrencySymbol: s = Resources.Sc; break;
                case UnicodeCategory.ModifierSymbol: s = Resources.Sk; break;
                case UnicodeCategory.OtherSymbol: s = Resources.So; break;
                case UnicodeCategory.OtherNotAssigned: s = Resources.Cn; break;
            }

            return s;
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
            private string Name { get; set; }
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


            public static bool FontEquals(Font left, Font right)
            {
                Debug.Assert(left != null);
                Debug.Assert(right != null);

                return (left.FontFamily.Equals(right.FontFamily)) &&
                        (left.SizeInPoints == right.SizeInPoints) &&
                        (left.Style == right.Style);
            }
        }
    }
}
