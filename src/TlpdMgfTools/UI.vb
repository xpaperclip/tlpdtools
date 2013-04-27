Imports System
Imports System.Drawing
Imports System.Windows.Forms

Public Class UI
    Public Shared Function InputBox(ByVal owner As IWin32Window,
                                    ByVal title As String, ByVal promptText As String,
                                    ByRef value As String) As DialogResult
        Dim form As New Form
        Dim label As New Label
        Dim textBox As New TextBox
        Dim buttonOk As New Button
        Dim buttonCancel As New Button

        form.Text = title
        label.Text = promptText
        textBox.Text = value

        form.Font = New Font("Segoe UI", 9)

        buttonOk.Text = "OK"
        buttonCancel.Text = "Cancel"
        buttonOk.DialogResult = DialogResult.OK
        buttonCancel.DialogResult = DialogResult.Cancel

        label.SetBounds(9, 20, 372, 13)
        textBox.SetBounds(12, 36, 372, 20)
        buttonOk.SetBounds(228, 72, 75, 23)
        buttonCancel.SetBounds(309, 72, 75, 23)

        label.AutoSize = True
        textBox.Anchor = textBox.Anchor Or AnchorStyles.Right
        buttonOk.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        buttonCancel.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right

        form.ClientSize = New Size(396, 107)
        form.Controls.AddRange(New Control() {textBox, label, buttonOk, buttonCancel})
        form.ClientSize = New Size(Math.Max(300, label.Right + 10), form.ClientSize.Height)
        form.FormBorderStyle = FormBorderStyle.FixedDialog
        form.StartPosition = FormStartPosition.CenterParent
        form.MinimizeBox = False
        form.MaximizeBox = False
        form.AcceptButton = buttonOk
        form.CancelButton = buttonCancel

        Dim result As DialogResult = form.ShowDialog(owner)
        value = textBox.Text
        Return result
    End Function
End Class
