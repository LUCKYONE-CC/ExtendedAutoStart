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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            lab_pIES = new Label();
            label1 = new Label();
            lV_programsInNormalStartup = new ListView();
            cMS_lVNormalStartup = new ContextMenuStrip(components);
            removeFromStartupToolStripMenuItem = new ToolStripMenuItem();
            openLocationToolStripMenuItem = new ToolStripMenuItem();
            lV_programsInExtendedStartup = new ListView();
            cMS_lVExtendedStartup = new ContextMenuStrip(components);
            addNewProgramToolStripMenuItem = new ToolStripMenuItem();
            removeToolStripMenuItem = new ToolStripMenuItem();
            activateDeactivateToolStripMenuItem = new ToolStripMenuItem();
            contextMenuStrip1 = new ContextMenuStrip(components);
            toolStripMenuItem1 = new ToolStripMenuItem();
            toolStripMenuItem2 = new ToolStripMenuItem();
            btn_transfer = new Button();
            toolTip1 = new ToolTip(components);
            notifyIcon = new NotifyIcon(components);
            cMS_lVNormalStartup.SuspendLayout();
            cMS_lVExtendedStartup.SuspendLayout();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
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
            // lV_programsInExtendedStartup
            // 
            lV_programsInExtendedStartup.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            lV_programsInExtendedStartup.ContextMenuStrip = cMS_lVExtendedStartup;
            lV_programsInExtendedStartup.FullRowSelect = true;
            lV_programsInExtendedStartup.Location = new Point(12, 48);
            lV_programsInExtendedStartup.Name = "lV_programsInExtendedStartup";
            lV_programsInExtendedStartup.Size = new Size(269, 379);
            lV_programsInExtendedStartup.TabIndex = 7;
            lV_programsInExtendedStartup.UseCompatibleStateImageBehavior = false;
            // 
            // cMS_lVExtendedStartup
            // 
            cMS_lVExtendedStartup.Items.AddRange(new ToolStripItem[] { addNewProgramToolStripMenuItem, removeToolStripMenuItem, activateDeactivateToolStripMenuItem });
            cMS_lVExtendedStartup.Name = "cMS_lVExtendedStartup";
            cMS_lVExtendedStartup.Size = new Size(178, 70);
            // 
            // addNewProgramToolStripMenuItem
            // 
            addNewProgramToolStripMenuItem.Name = "addNewProgramToolStripMenuItem";
            addNewProgramToolStripMenuItem.Size = new Size(177, 22);
            addNewProgramToolStripMenuItem.Text = "Add new program";
            addNewProgramToolStripMenuItem.Click += addNewProgramToolStripMenuItem_Click;
            // 
            // removeToolStripMenuItem
            // 
            removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            removeToolStripMenuItem.Size = new Size(177, 22);
            removeToolStripMenuItem.Text = "Remove";
            removeToolStripMenuItem.Click += removeToolStripMenuItem_Click;
            // 
            // activateDeactivateToolStripMenuItem
            // 
            activateDeactivateToolStripMenuItem.Name = "activateDeactivateToolStripMenuItem";
            activateDeactivateToolStripMenuItem.Size = new Size(177, 22);
            activateDeactivateToolStripMenuItem.Text = "Activate/Deactivate";
            activateDeactivateToolStripMenuItem.Click += activateDeactivateToolStripMenuItem_Click;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { toolStripMenuItem1, toolStripMenuItem2 });
            contextMenuStrip1.Name = "cMS_lVNormalStartup";
            contextMenuStrip1.Size = new Size(188, 48);
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(187, 22);
            toolStripMenuItem1.Text = "Remove from Startup";
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new Size(187, 22);
            toolStripMenuItem2.Text = "Open location";
            // 
            // btn_transfer
            // 
            btn_transfer.Location = new Point(353, 12);
            btn_transfer.Name = "btn_transfer";
            btn_transfer.Size = new Size(97, 23);
            btn_transfer.TabIndex = 8;
            btn_transfer.Text = "Transfer";
            toolTip1.SetToolTip(btn_transfer, "Transfer all programs from normal startup to extended startup");
            btn_transfer.UseVisualStyleBackColor = true;
            btn_transfer.Click += btn_transfer_Click;
            // 
            // notifyIcon
            // 
            notifyIcon.Icon = (Icon)resources.GetObject("notifyIcon.Icon");
            notifyIcon.Text = "ExtendedAutoStart";
            notifyIcon.Visible = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btn_transfer);
            Controls.Add(lV_programsInExtendedStartup);
            Controls.Add(lV_programsInNormalStartup);
            Controls.Add(label1);
            Controls.Add(lab_pIES);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Form1";
            Text = "ExtendedStartup";
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            cMS_lVNormalStartup.ResumeLayout(false);
            cMS_lVExtendedStartup.ResumeLayout(false);
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label lab_pIES;
        private Label label1;
        private ListView lV_programsInNormalStartup;
        private ContextMenuStrip cMS_lVNormalStartup;
        private ToolStripMenuItem removeFromStartupToolStripMenuItem;
        private ToolStripMenuItem openLocationToolStripMenuItem;
        private ListView lV_programsInExtendedStartup;
        private ContextMenuStrip cMS_lVExtendedStartup;
        private ToolStripMenuItem addNewProgramToolStripMenuItem;
        private ToolStripMenuItem removeToolStripMenuItem;
        private ToolStripMenuItem activateDeactivateToolStripMenuItem;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem toolStripMenuItem1;
        private ToolStripMenuItem toolStripMenuItem2;
        private Button btn_transfer;
        private ToolTip toolTip1;
        private NotifyIcon notifyIcon;
    }
}
