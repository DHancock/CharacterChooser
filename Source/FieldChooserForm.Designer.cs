namespace FieldChooser
{
    partial class FieldChooserForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.fieldComboBox = new System.Windows.Forms.ComboBox();
            this.fieldComboLabel = new System.Windows.Forms.Label();
            this.indexComboBox1 = new System.Windows.Forms.ComboBox();
            this.indexComboBox2 = new System.Windows.Forms.ComboBox();
            this.indexComboBox3 = new System.Windows.Forms.ComboBox();
            this.indexComboBox4 = new System.Windows.Forms.ComboBox();
            this.charTextBox1 = new System.Windows.Forms.TextBox();
            this.charTextBox2 = new System.Windows.Forms.TextBox();
            this.charTextBox3 = new System.Windows.Forms.TextBox();
            this.charTextBox4 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.protectButton = new System.Windows.Forms.Button();
            this.copyButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // fieldComboBox
            // 
            this.fieldComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.fieldComboBox.Location = new System.Drawing.Point(59, 27);
            this.fieldComboBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.fieldComboBox.Name = "fieldComboBox";
            this.fieldComboBox.Size = new System.Drawing.Size(238, 24);
            this.fieldComboBox.TabIndex = 1;
            this.fieldComboBox.SelectedIndexChanged += new System.EventHandler(this.FieldComboBox_SelectedIndexChanged);
            // 
            // fieldComboLabel
            // 
            this.fieldComboLabel.AutoSize = true;
            this.fieldComboLabel.Location = new System.Drawing.Point(15, 31);
            this.fieldComboLabel.Name = "fieldComboLabel";
            this.fieldComboLabel.Size = new System.Drawing.Size(38, 17);
            this.fieldComboLabel.TabIndex = 0;
            this.fieldComboLabel.Text = "Field";
            // 
            // indexComboBox1
            // 
            this.indexComboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.indexComboBox1.Items.AddRange(new object[] {
            "-"});
            this.indexComboBox1.Location = new System.Drawing.Point(183, 81);
            this.indexComboBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.indexComboBox1.Name = "indexComboBox1";
            this.indexComboBox1.Size = new System.Drawing.Size(48, 24);
            this.indexComboBox1.TabIndex = 4;
            this.indexComboBox1.SelectedIndexChanged += new System.EventHandler(this.IndexComboBox_SelectedIndexChanged);
            // 
            // indexComboBox2
            // 
            this.indexComboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.indexComboBox2.Items.AddRange(new object[] {
            "-"});
            this.indexComboBox2.Location = new System.Drawing.Point(183, 112);
            this.indexComboBox2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.indexComboBox2.Name = "indexComboBox2";
            this.indexComboBox2.Size = new System.Drawing.Size(48, 24);
            this.indexComboBox2.TabIndex = 8;
            this.indexComboBox2.SelectedIndexChanged += new System.EventHandler(this.IndexComboBox_SelectedIndexChanged);
            // 
            // indexComboBox3
            // 
            this.indexComboBox3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.indexComboBox3.FormattingEnabled = true;
            this.indexComboBox3.Items.AddRange(new object[] {
            "-"});
            this.indexComboBox3.Location = new System.Drawing.Point(183, 142);
            this.indexComboBox3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.indexComboBox3.Name = "indexComboBox3";
            this.indexComboBox3.Size = new System.Drawing.Size(48, 24);
            this.indexComboBox3.TabIndex = 10;
            this.indexComboBox3.SelectedIndexChanged += new System.EventHandler(this.IndexComboBox_SelectedIndexChanged);
            // 
            // indexComboBox4
            // 
            this.indexComboBox4.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.indexComboBox4.FormattingEnabled = true;
            this.indexComboBox4.Items.AddRange(new object[] {
            "-"});
            this.indexComboBox4.Location = new System.Drawing.Point(183, 173);
            this.indexComboBox4.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.indexComboBox4.Name = "indexComboBox4";
            this.indexComboBox4.Size = new System.Drawing.Size(48, 24);
            this.indexComboBox4.TabIndex = 12;
            this.indexComboBox4.SelectedIndexChanged += new System.EventHandler(this.IndexComboBox_SelectedIndexChanged);
            // 
            // charTextBox1
            // 
            this.charTextBox1.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.charTextBox1.Enabled = false;
            this.charTextBox1.Location = new System.Drawing.Point(264, 82);
            this.charTextBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.charTextBox1.Name = "charTextBox1";
            this.charTextBox1.ReadOnly = true;
            this.charTextBox1.Size = new System.Drawing.Size(33, 22);
            this.charTextBox1.TabIndex = 6;
            this.charTextBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.charTextBox1.TextChanged += new System.EventHandler(this.CharTextBox_TextChanged);
            // 
            // charTextBox2
            // 
            this.charTextBox2.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.charTextBox2.Enabled = false;
            this.charTextBox2.Location = new System.Drawing.Point(264, 112);
            this.charTextBox2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.charTextBox2.Name = "charTextBox2";
            this.charTextBox2.ReadOnly = true;
            this.charTextBox2.Size = new System.Drawing.Size(33, 22);
            this.charTextBox2.TabIndex = 9;
            this.charTextBox2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.charTextBox2.TextChanged += new System.EventHandler(this.CharTextBox_TextChanged);
            // 
            // charTextBox3
            // 
            this.charTextBox3.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.charTextBox3.Enabled = false;
            this.charTextBox3.Location = new System.Drawing.Point(264, 142);
            this.charTextBox3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.charTextBox3.Name = "charTextBox3";
            this.charTextBox3.ReadOnly = true;
            this.charTextBox3.Size = new System.Drawing.Size(33, 22);
            this.charTextBox3.TabIndex = 11;
            this.charTextBox3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.charTextBox3.TextChanged += new System.EventHandler(this.CharTextBox_TextChanged);
            // 
            // charTextBox4
            // 
            this.charTextBox4.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.charTextBox4.Enabled = false;
            this.charTextBox4.Location = new System.Drawing.Point(264, 172);
            this.charTextBox4.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.charTextBox4.Name = "charTextBox4";
            this.charTextBox4.ReadOnly = true;
            this.charTextBox4.Size = new System.Drawing.Size(33, 22);
            this.charTextBox4.TabIndex = 13;
            this.charTextBox4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.charTextBox4.TextChanged += new System.EventHandler(this.CharTextBox_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(56, 85);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(122, 17);
            this.label1.TabIndex = 3;
            this.label1.Text = "Character number";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(240, 85);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(18, 17);
            this.label2.TabIndex = 5;
            this.label2.Text = "is";
            // 
            // protectButton
            // 
            this.protectButton.Enabled = false;
            this.protectButton.Image = global::FieldChooser.Properties.Resources.Dots;
            this.protectButton.Location = new System.Drawing.Point(312, 81);
            this.protectButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.protectButton.Name = "protectButton";
            this.protectButton.Size = new System.Drawing.Size(35, 25);
            this.protectButton.TabIndex = 7;
            this.protectButton.UseVisualStyleBackColor = true;
            this.protectButton.EnabledChanged += new System.EventHandler(this.ProtectButtonEnabledStateChanged);
            this.protectButton.Click += new System.EventHandler(this.ProtectButton_Click);
            // 
            // copyButton
            // 
            this.copyButton.Image = global::FieldChooser.Properties.Resources.Copy;
            this.copyButton.Location = new System.Drawing.Point(312, 27);
            this.copyButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.copyButton.Name = "copyButton";
            this.copyButton.Size = new System.Drawing.Size(35, 25);
            this.copyButton.TabIndex = 2;
            this.copyButton.UseVisualStyleBackColor = true;
            this.copyButton.Click += new System.EventHandler(this.CopyButton_Click);
            // 
            // FieldChooserForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(366, 214);
            this.Controls.Add(this.protectButton);
            this.Controls.Add(this.copyButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.charTextBox4);
            this.Controls.Add(this.charTextBox3);
            this.Controls.Add(this.charTextBox2);
            this.Controls.Add(this.charTextBox1);
            this.Controls.Add(this.indexComboBox4);
            this.Controls.Add(this.indexComboBox3);
            this.Controls.Add(this.indexComboBox2);
            this.Controls.Add(this.indexComboBox1);
            this.Controls.Add(this.fieldComboLabel);
            this.Controls.Add(this.fieldComboBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FieldChooserForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Field Chooser";
            this.Load += new System.EventHandler(this.FieldChooserForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox fieldComboBox;
        private System.Windows.Forms.Label fieldComboLabel;
        private System.Windows.Forms.ComboBox indexComboBox1;
        private System.Windows.Forms.ComboBox indexComboBox2;
        private System.Windows.Forms.ComboBox indexComboBox3;
        private System.Windows.Forms.ComboBox indexComboBox4;
        private System.Windows.Forms.TextBox charTextBox1;
        private System.Windows.Forms.TextBox charTextBox2;
        private System.Windows.Forms.TextBox charTextBox3;
        private System.Windows.Forms.TextBox charTextBox4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button copyButton;
        private System.Windows.Forms.Button protectButton;
    }
}

