<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class DownloadProgressForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.lblText = New System.Windows.Forms.Label()
        Me.barProgress = New System.Windows.Forms.ProgressBar()
        Me.picImage = New System.Windows.Forms.PictureBox()
        Me.lnkCancel = New System.Windows.Forms.LinkLabel()
        CType(Me.picImage, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'lblText
        '
        Me.lblText.AutoSize = True
        Me.lblText.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblText.Location = New System.Drawing.Point(38, 27)
        Me.lblText.Name = "lblText"
        Me.lblText.Size = New System.Drawing.Size(90, 13)
        Me.lblText.TabIndex = 1
        Me.lblText.Text = "Downloading ..."
        Me.lblText.UseWaitCursor = True
        '
        'barProgress
        '
        Me.barProgress.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.barProgress.Location = New System.Drawing.Point(41, 52)
        Me.barProgress.Name = "barProgress"
        Me.barProgress.Size = New System.Drawing.Size(284, 16)
        Me.barProgress.Style = System.Windows.Forms.ProgressBarStyle.Marquee
        Me.barProgress.TabIndex = 2
        Me.barProgress.UseWaitCursor = True
        '
        'picImage
        '
        Me.picImage.Location = New System.Drawing.Point(12, 23)
        Me.picImage.Name = "picImage"
        Me.picImage.Size = New System.Drawing.Size(20, 20)
        Me.picImage.TabIndex = 3
        Me.picImage.TabStop = False
        Me.picImage.UseWaitCursor = True
        '
        'lnkCancel
        '
        Me.lnkCancel.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lnkCancel.AutoSize = True
        Me.lnkCancel.Location = New System.Drawing.Point(284, 99)
        Me.lnkCancel.Name = "lnkCancel"
        Me.lnkCancel.Size = New System.Drawing.Size(41, 13)
        Me.lnkCancel.TabIndex = 4
        Me.lnkCancel.TabStop = True
        Me.lnkCancel.Text = "Cancel"
        Me.lnkCancel.UseWaitCursor = True
        '
        'DownloadProgressForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.lnkCancel
        Me.ClientSize = New System.Drawing.Size(342, 142)
        Me.ControlBox = False
        Me.Controls.Add(Me.lnkCancel)
        Me.Controls.Add(Me.barProgress)
        Me.Controls.Add(Me.lblText)
        Me.Controls.Add(Me.picImage)
        Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "DownloadProgressForm"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Please wait"
        Me.UseWaitCursor = True
        CType(Me.picImage, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents lblText As System.Windows.Forms.Label
    Friend WithEvents barProgress As System.Windows.Forms.ProgressBar
    Friend WithEvents picImage As System.Windows.Forms.PictureBox
    Friend WithEvents lnkCancel As System.Windows.Forms.LinkLabel
End Class
