namespace CSS_Cheat
{
    partial class ModuleListForm
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
            moduleListBox = new ListBox();
            SuspendLayout();
            // 
            // moduleListBox
            // 
            moduleListBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            moduleListBox.FormattingEnabled = true;
            moduleListBox.ItemHeight = 17;
            moduleListBox.Location = new Point(7, 9);
            moduleListBox.Name = "moduleListBox";
            moduleListBox.Size = new Size(340, 429);
            moduleListBox.TabIndex = 0;
            // 
            // ModuleListForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(359, 450);
            ControlBox = false;
            Controls.Add(moduleListBox);
            Name = "ModuleListForm";
            Text = "模块组件列表";
            ResumeLayout(false);
        }

        #endregion

        private ListBox moduleListBox;
    }
}