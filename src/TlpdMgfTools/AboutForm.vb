Imports System
Imports System.Windows.Forms

Class AboutForm
    Inherits Form

    Sub New()
        InitializeComponent()
    End Sub

    Private Sub AboutForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        WebBrowser1.Navigate("about:blank")
        WebBrowser1.Document.Write(String.Empty)
        Dim text As String = EmbeddedResources.about
        text = text.Replace("{about/version}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString())
        WebBrowser1.DocumentText = text
    End Sub

    Private Sub lnkOK_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles lnkOK.LinkClicked
        Me.Close()
    End Sub

End Class
