Imports System
Imports System.Drawing
Imports System.Net
Imports System.Windows.Forms
Imports System.IO

Public Class DownloadProgressForm
    Inherits Form

    Public Shared Function BeginDownload(url As String) As DownloadProgressForm
        Dim request As HttpWebRequest = CType(WebRequest.Create(url), HttpWebRequest)
        request.Timeout = 10000
        Return New DownloadProgressForm(request)
    End Function

    Public Sub New(httpRequest As HttpWebRequest)
        InitializeComponent()

        'default image
        picImage.Image = EmbeddedResources.wiki_icon
        _request = httpRequest
    End Sub

    Private _request As HttpWebRequest
    Private _aborted As Boolean = False
    Private _result As String

    Public ReadOnly Property Aborted As Boolean
        Get
            Return _aborted
        End Get
    End Property
    Public ReadOnly Property Result As String
        Get
            Return _result
        End Get
    End Property

    Public Property Image As Image
        Get
            Return picImage.Image
        End Get
        Set(Value As Image)
            picImage.Image = Value
        End Set
    End Property

    Private Sub DownloadProgressForm_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        'workaround for BeginGetReponse still blocking under some circumstances
        Threading.ThreadPool.QueueUserWorkItem(Sub()
                                                   _request.BeginGetResponse(AddressOf Response_Callback, _request)
                                               End Sub)
    End Sub
    Private Sub Response_Callback(ByVal ar As IAsyncResult)
        Dim request As HttpWebRequest = CType(ar.AsyncState, HttpWebRequest)
        Dim response As WebResponse
        Try
            response = request.EndGetResponse(ar)
        Catch ex As Exception
            If Aborted Then Return

            MessageBox.Show(ex.ToString(), "Error")
            CloseMe(Windows.Forms.DialogResult.Cancel)
            Return
        End Try
        If Aborted Then Return

        Dim length As Integer = CInt(response.ContentLength)
        InitialiseProgress(length)

        Try
            Using responseStream As Stream = response.GetResponseStream()
                If length < 0 Then
                    ' progress unsupported
                    Using sr As New StreamReader(responseStream)
                        _result = sr.ReadToEnd()
                        CloseMe(Windows.Forms.DialogResult.OK)
                        Return
                    End Using
                End If

                Dim bytes(length) As Byte
                Dim sofar As Integer = 0
                Dim count As Integer
                While True
                    If Aborted Then
                        'cancel
                        UpdateProgress(sofar, length)
                        Exit While
                    End If

                    count = responseStream.Read(bytes, sofar, Math.Min(4096, length - sofar))
                    sofar += count

                    UpdateProgress(sofar, length)

                    If sofar = length OrElse count = 0 Then
                        'done
                        _result = System.Text.Encoding.Default.GetString(bytes)
                        CloseMe(Windows.Forms.DialogResult.OK)
                        Return
                    End If
                End While
            End Using
        Catch ex As WebException
            If Not Aborted Then
                MessageBox.Show(ex.ToString(), "Error while downloading data", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
            CloseMe(Windows.Forms.DialogResult.Cancel)
            Return
        End Try
    End Sub

    Private Sub InitialiseProgress(ByVal length As Integer)
        If Me.InvokeRequired Then
            Me.Invoke(New Action(Of Integer)(AddressOf InitialiseProgress), length)
            Return
        End If

        If length = -1 Then
            ' don't know
            barProgress.Style = ProgressBarStyle.Marquee
        Else
            barProgress.Style = ProgressBarStyle.Blocks
            barProgress.Maximum = length
        End If
    End Sub
    Private Sub UpdateProgress(ByVal sofar As Integer, ByVal length As Integer)
        If Me.InvokeRequired Then
            Me.Invoke(New Action(Of Integer, Integer)(AddressOf UpdateProgress), sofar, length)
            Return
        End If
        barProgress.Value = sofar

        If length > 0 Then
            lblText.Text = String.Format("Downloading ... ({0:0} of {1:0} kB)", _
                                             sofar / 1024, length / 1024)
            barProgress.Value = sofar
        Else
            lblText.Text = String.Format("Downloading ... ({0:0} kB)", _
                                             sofar / 1024, length / 1024)
        End If
    End Sub
    Private Sub CloseMe(dialogResult As DialogResult)
        If Me.InvokeRequired Then
            Me.Invoke(New Action(Of DialogResult)(AddressOf CloseMe), dialogResult)
            Return
        End If

        Me.DialogResult = dialogResult
        Me.Close()
    End Sub

    Private Sub lnkCancel_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles lnkCancel.LinkClicked
        _aborted = True
        _request.Abort()
        CloseMe(Windows.Forms.DialogResult.Cancel)
    End Sub

End Class