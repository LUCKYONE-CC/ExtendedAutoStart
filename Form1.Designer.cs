namespace ExtendedAutoStart
{
    partial class Form1
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
            components = new System.ComponentModel.Container();
            notifyIcon = new NotifyIcon(components);
            cMS_NI = new ContextMenuStrip(components);
            openPanelToolStripMenuItem = new ToolStripMenuItem();
            lab_pIES = new Label();
            lB_programsInExtendedStartup = new ListBox();
            label1 = new Label();
            lV_programsInNormalStartup = new ListView();
            cMS_lVNormalStartup = new ContextMenuStrip(components);
            removeFromStartupToolStripMenuItem = new ToolStripMenuItem();
            openLocationToolStripMenuItem = new ToolStripMenuItem();
            cMS_NI.SuspendLayout();
            cMS_lVNormalStartup.SuspendLayout();
            SuspendLayout();
            // 
            // notifyIcon
            // 
            notifyIcon.Text = "ExtendedStartupNotifyIcon";
            notifyIcon.Visible = true;
            // 
            // cMS_NI
            // 
            cMS_NI.Items.AddRange(new ToolStripItem[] { openPanelToolStripMenuItem });
            cMS_NI.Name = "cMS_NI";
            cMS_NI.Size = new Size(136, 26);
            // 
            // openPanelToolStripMenuItem
            // 
            openPanelToolStripMenuItem.Name = "openPanelToolStripMenuItem";
            openPanelToolStripMenuItem.Size = new Size(135, 22);
            openPanelToolStripMenuItem.Text = "Open Panel";
            // 
            // lab_pIES
            // 
            lab_pIES.AutoSize = true;
            lab_pIES.Font = new Font("Segoe UI", 12F);
            lab_pIES.Location = new Point(12, 9);
            lab_pIES.Name = "lab_pIES";
            lab_pIES.Size = new Size(216, 21);
            lab_pIES.TabIndex = 2;
            lab_pIES.Text = "Programs in Extended Startup";
            // 
            // lB_programsInExtendedStartup
            // 
            lB_programsInExtendedStartup.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            lB_programsInExtendedStartup.FormattingEnabled = true;
            lB_programsInExtendedStartup.ItemHeight = 15;
            lB_programsInExtendedStartup.Location = new Point(12, 48);
            lB_programsInExtendedStartup.Name = "lB_programsInExtendedStartup";
            lB_programsInExtendedStartup.Size = new Size(269, 379);
            lB_programsInExtendedStartup.TabIndex = 3;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 12F);
            label1.Location = new Point(582, 9);
            label1.Name = "label1";
            label1.Size = new Size(206, 21);
            label1.TabIndex = 5;
            label1.Text = "Programs in Normal Startup";
            // 
            // lV_programsInNormalStartup
            // 
            lV_programsInNormalStartup.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            lV_programsInNormalStartup.ContextMenuStrip = cMS_lVNormalStartup;
            lV_programsInNormalStartup.FullRowSelect = true;
            lV_programsInNormalStartup.Location = new Point(519, 48);
            lV_programsInNormalStartup.Name = "lV_programsInNormalStartup";
            lV_programsInNormalStartup.Size = new Size(269, 379);
            lV_programsInNormalStartup.TabIndex = 6;
            lV_programsInNormalStartup.UseCompatibleStateImageBehavior = false;
            // 
            // cMS_lVNormalStartup
            // 
            cMS_lVNormalStartup.Items.AddRange(new ToolStripItem[] { removeFromStartupToolStripMenuItem, openLocationToolStripMenuItem });
            cMS_lVNormalStartup.Name = "cMS_lVNormalStartup";
            cMS_lVNormalStartup.Size = new Size(188, 48);
            // 
            // removeFromStartupToolStripMenuItem
            // 
            removeFromStartupToolStripMenuItem.Name = "removeFromStartupToolStripMenuItem";
            removeFromStartupToolStripMenuItem.Size = new Size(187, 22);
            removeFromStartupToolStripMenuItem.Text = "Remove from Startup";
            removeFromStartupToolStripMenuItem.Click += removeFromStartupToolStripMenuItem_Click;
            // 
            // openLocationToolStripMenuItem
            // 
            openLocationToolStripMenuItem.Name = "openLocationToolStripMenuItem";
            openLocationToolStripMenuItem.Size = new Size(187, 22);
            openLocationToolStripMenuItem.Text = "Open location";
            openLocationToolStripMenuItem.Click += openLocationToolStripMenuItem_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(lV_programsInNormalStartup);
            Controls.Add(label1);
            Controls.Add(lB_programsInExtendedStartup);
            Controls.Add(lab_pIES);
            Name = "Form1";
            Text = "ExtendedStartup";
            Load += Form1_Load;
            cMS_NI.ResumeLayout(false);
            cMS_lVNormalStartup.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private NotifyIcon notifyIcon;
        private ContextMenuStrip cMS_NI;
        private ToolStripMenuItem openPanelToolStripMenuItem;
        private Label lab_pIES;
        private ListBox lB_programsInExtendedStartup;
        private Label label1;
        private ListView lV_programsInNormalStartup;
        private ContextMenuStrip cMS_lVNormalStartup;
        private ToolStripMenuItem removeFromStartupToolStripMenuItem;
        private ToolStripMenuItem openLocationToolStripMenuItem;
    }
}
