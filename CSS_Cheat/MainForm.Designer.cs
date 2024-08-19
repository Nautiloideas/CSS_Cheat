namespace CSS_Cheat
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            processListBox = new ListBox();
            selectProcessButton = new Button();
            refreshProcessListButton = new Button();
            btnStartDrawing = new Button();
            btnStopDrawing = new Button();
            SuspendLayout();
            // 
            // processListBox
            // 
            processListBox.FormattingEnabled = true;
            processListBox.ItemHeight = 17;
            processListBox.Location = new Point(12, 12);
            processListBox.Name = "processListBox";
            processListBox.Size = new Size(459, 310);
            processListBox.TabIndex = 0;
            // 
            // selectProcessButton
            // 
            selectProcessButton.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            selectProcessButton.Location = new Point(253, 338);
            selectProcessButton.Name = "selectProcessButton";
            selectProcessButton.Size = new Size(168, 38);
            selectProcessButton.TabIndex = 1;
            selectProcessButton.Text = "选择该进程";
            selectProcessButton.UseVisualStyleBackColor = true;
            selectProcessButton.Click += selectProcessButton_Click;
            // 
            // refreshProcessListButton
            // 
            refreshProcessListButton.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            refreshProcessListButton.Location = new Point(25, 338);
            refreshProcessListButton.Name = "refreshProcessListButton";
            refreshProcessListButton.Size = new Size(168, 38);
            refreshProcessListButton.TabIndex = 2;
            refreshProcessListButton.Text = "刷新进程列表";
            refreshProcessListButton.UseVisualStyleBackColor = true;
            refreshProcessListButton.Click += refreshProcessListButton_Click;
            // 
            // btnStartDrawing
            // 
            btnStartDrawing.Location = new Point(25, 382);
            btnStartDrawing.Name = "btnStartDrawing";
            btnStartDrawing.Size = new Size(168, 38);
            btnStartDrawing.TabIndex = 3;
            btnStartDrawing.Text = "开始绘制";
            btnStartDrawing.UseVisualStyleBackColor = true;
            btnStartDrawing.Click += btnStartDrawing_Click;
            // 
            // btnStopDrawing
            // 
            btnStopDrawing.Location = new Point(253, 386);
            btnStopDrawing.Name = "btnStopDrawing";
            btnStopDrawing.Size = new Size(168, 34);
            btnStopDrawing.TabIndex = 4;
            btnStopDrawing.Text = "结束绘制";
            btnStopDrawing.UseVisualStyleBackColor = true;
            btnStopDrawing.Click += btnStopDrawing_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(481, 432);
            Controls.Add(btnStopDrawing);
            Controls.Add(btnStartDrawing);
            Controls.Add(refreshProcessListButton);
            Controls.Add(selectProcessButton);
            Controls.Add(processListBox);
            Name = "MainForm";
            Text = "CSS Cheat";
            ResumeLayout(false);
        }

        #endregion

        private ListBox processListBox;
        private Button selectProcessButton;
        private Button refreshProcessListButton;
        private Button btnStartDrawing;
        private Button btnStopDrawing;
    }
}
