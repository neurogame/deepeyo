<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.canvas = New System.Windows.Forms.PictureBox()
        Me.game = New System.Windows.Forms.Timer(Me.components)
        CType(Me.canvas, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'canvas
        '
        Me.canvas.BackColor = System.Drawing.SystemColors.Window
        Me.canvas.Dock = System.Windows.Forms.DockStyle.Fill
        Me.canvas.Location = New System.Drawing.Point(0, 0)
        Me.canvas.Name = "canvas"
        Me.canvas.Size = New System.Drawing.Size(630, 420)
        Me.canvas.TabIndex = 0
        Me.canvas.TabStop = False
        '
        'game
        '
        Me.game.Interval = 10
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(630, 420)
        Me.Controls.Add(Me.canvas)
        Me.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.Name = "Form1"
        Me.ShowIcon = False
        Me.Text = "deepeyo"
        CType(Me.canvas, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents canvas As PictureBox
    Friend WithEvents game As Timer
End Class
