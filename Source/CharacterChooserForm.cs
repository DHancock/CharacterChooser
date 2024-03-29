﻿/*
Copyright 2019-2022 David Hancock

This file is part of the CharacterChooser plugin for KeePass 2.

CharacterChooser is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 2 of the License, or
(at your option) any later version. 

CharacterChooser is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with CharacterChooser.  If not, see<https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Windows.Forms; 
using System.Diagnostics;
using System.Globalization;
using System.Drawing;
using System.Windows.Forms.VisualStyles;

using KeePass.Plugins;
using KeePass.Util;
using KeePassLib.Security;
using KeePass.App;
using KeePassLib.Utility;

using CharacterChooser.Properties;


namespace CharacterChooser
{
    public partial class CharacterChooserForm : Form
    {
        private IPluginHost Host { get; set; }

        private readonly List<CharacterSelectorRow> characterSelectorRows = new List<CharacterSelectorRow>();
        private ProtectedString fieldString;
        private readonly Color uppercaseHiliteColor = Color.Red;

        private const string cWidthKey = "CharacterChooser.DialogInfo.Width";
        private const string cXKey = "CharacterChooser.DialogInfo.X";
        private const string cYKey = "CharacterChooser.DialogInfo.Y";


        private CharacterChooserForm()
        {
            InitializeComponent();
        }


        internal CharacterChooserForm(IPluginHost host, ToolStripItemCollection fields, ToolStripItem startField) : this()
        {
            Host = host;

            // the standard KeePass app icon
            Icon = AppIcons.Default;

            // link a index combo box with its associated text box
            characterSelectorRows.Add(new CharacterSelectorRow(indexComboBox1, charTextBox1));
            characterSelectorRows.Add(new CharacterSelectorRow(indexComboBox2, charTextBox2));
            characterSelectorRows.Add(new CharacterSelectorRow(indexComboBox3, charTextBox3));
            characterSelectorRows.Add(new CharacterSelectorRow(indexComboBox4, charTextBox4));

            // build the field combo box items
            foreach (ToolStripItem item in fields)
                fieldComboBox.Items.Add(item);

            fieldComboBox.SelectedItem = startField;
            fieldComboBox.DisplayMember = "Text";
        }


        private void CharacterChooserForm_Load(object sender, EventArgs e)
        {
            // adjust the layout as required
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

            if (!Utils.EqivalentFont(passwordFont, charTextBox1.Font))
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


        private void CharacterChooserForm_FormClosing(object sender, FormClosingEventArgs e)
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
            object newString = ((ToolStripItem)fieldComboBox.SelectedItem).Tag;

            if (!ReferenceEquals(fieldString, newString))
            {
                fieldString = (ProtectedString)newString;

                protectButton.Enabled = false;

                foreach (CharacterSelectorRow row in characterSelectorRows)
                {
                    AdjustCharacterComboBoxItems(row.IndexComboBox, startIndex: 0);
                    LoadCharacterTextBox(row.IndexComboBox, row.CharTextBox);
                    row.CharTextBox.UseSystemPasswordChar = fieldString.IsProtected;
                }
            }
        }


        private void IndexComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool characterSelected = false;
            bool adjustRemainingComboBoxes = false;

            foreach (CharacterSelectorRow row in characterSelectorRows)
            {
                characterSelected |= row.IndexComboBox.SelectedIndex > 0;

                if (adjustRemainingComboBoxes)
                {
                    int startIndex = 0;
                    ComboBox indexComboBox = (ComboBox)sender;

                    if (indexComboBox.SelectedIndex == 0)
                    {
                        if (indexComboBox.Items.Count > 1)
                            startIndex = ((int)indexComboBox.Items[1]) - 1;
                    }
                    else
                        startIndex = (int)indexComboBox.SelectedItem;

                    AdjustCharacterComboBoxItems(row.IndexComboBox, startIndex);
                    LoadCharacterTextBox(row.IndexComboBox, row.CharTextBox);
                }
                else if (ReferenceEquals(row.IndexComboBox, sender))
                {
                    adjustRemainingComboBoxes = true;
                    LoadCharacterTextBox(row.IndexComboBox, row.CharTextBox);
                }
            }

            protectButton.Enabled = fieldString.IsProtected && characterSelected;
        }


        private void AdjustCharacterComboBoxItems(ComboBox indexComboBox, int startIndex)
        {
            indexComboBox.SelectedIndexChanged -= IndexComboBox_SelectedIndexChanged;
            indexComboBox.Items.Clear();
            indexComboBox.Items.Add(Resources.character_combo_item_zero);

            for (int index = startIndex + 1; index <= fieldString.Length; ++index)
                indexComboBox.Items.Add(index);

            indexComboBox.SelectedIndex = 0;
            indexComboBox.Enabled = indexComboBox.Items.Count > 1;
            indexComboBox.SelectedIndexChanged += IndexComboBox_SelectedIndexChanged;
        }


        private void LoadCharacterTextBox(ComboBox indexComboBox, TextBox textBox)
        {
            // the first combo box item is a dash indicating that there is no character selected
            if (indexComboBox.SelectedIndex == 0)
                textBox.Text = string.Empty;
            else
            {
                char[] chars = null;

                try
                {
                    chars = fieldString.ReadChars();
                    textBox.Text = chars[((int)indexComboBox.SelectedItem) - 1].ToString(CultureInfo.CurrentCulture);
                }
                finally
                {
                    MemUtil.ZeroArray(chars);
                }
            }
        }


        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if ((keyData == Keys.Escape) || 
                (keyData == (Keys.Control | Keys.W)) ||
                (keyData == (Keys.Alt | Keys.F4)))
            {
                Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }


        private void CopyButton_Click(object sender, EventArgs e)
        {
            if (ClipboardUtil.CopyAndMinimize(fieldString, false, this, null, null))
                Host.MainWindow.StartClipboardCountdown();
        }


        private void ProtectButton_Click(object sender, EventArgs e)
        {
            foreach (CharacterSelectorRow row in characterSelectorRows)
            {
                TextBox tb = row.CharTextBox;

                tb.UseSystemPasswordChar = !tb.UseSystemPasswordChar;

                if (tb.UseSystemPasswordChar)
                    SetForeColor(tb, Color.Black);
                else if ((tb.Text.Length > 0) && (Char.GetUnicodeCategory(tb.Text[0]) == UnicodeCategory.UppercaseLetter))
                    SetForeColor(tb, uppercaseHiliteColor);
            }
        }

        private static void SetForeColor(TextBox tb, Color color)
        {
            if (tb.ForeColor != color)
                tb.ForeColor = color;
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
            TextBox tb = (TextBox)sender;

            if (string.IsNullOrEmpty(tb.Text))
                tb.Enabled = false;
            else
            {
                tb.Enabled = true;
                toolTip.SetToolTip(tb, GetUnicodeCategoryDescription(tb.Text[0]));
                toolTip.AutomaticDelay = 125;

                if (!tb.UseSystemPasswordChar && (Char.GetUnicodeCategory(tb.Text[0]) == UnicodeCategory.UppercaseLetter))
                    SetForeColor(tb, uppercaseHiliteColor); 
                else
                    SetForeColor(tb, Color.Black);
            }
        }


        private void ToolTip_Popup(object sender, PopupEventArgs e)
        {
            e.Cancel = ((TextBox)e.AssociatedControl).UseSystemPasswordChar;
        }


        /// <summary>
        /// Gets a description of the Unicode category that the supplied
        /// character belongs to. Helps the user identify which look a 
        /// like character they are dealing with.
        /// </summary>
        /// <param name="c"></param>
        /// <returns>category description</returns>
        private static string GetUnicodeCategoryDescription(char c)
        {
            string s ;
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
                default: s = string.Empty; break;
            }

            return s;
        }


        private sealed class CharacterSelectorRow
        {
            public ComboBox IndexComboBox { get; private set; }
            public TextBox CharTextBox { get; private set; }

            public CharacterSelectorRow(ComboBox comboBox, TextBox textBox)
            {
                IndexComboBox = comboBox;
                CharTextBox = textBox;
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
            

            // Check fonts are the same. The Font.Equals() method 
            // is too strict.
            public static bool EqivalentFont(Font left, Font right)
            {
                if (ReferenceEquals(left, right))
                    return true;
                
                if ((left == null) || (right == null))
                    return false;

                // the font family should never be null
                return (left.FontFamily.Equals(right.FontFamily)) &&
                        (left.SizeInPoints == right.SizeInPoints) &&
                        (left.Style == right.Style);
            }
        }
    }
}
