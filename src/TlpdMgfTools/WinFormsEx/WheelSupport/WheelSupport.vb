Imports System
Imports System.Drawing
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Windows.Forms

Public Class WheelSupport
    Inherits NativeWindow
    Implements IMessageFilter

    Private WithEvents parent As Form

    Public Sub New(ByVal f As Form)
        Me.parent = f
    End Sub

    Private Sub OnParentActivated(ByVal sender As Object, ByVal e As EventArgs) Handles parent.Activated
        Application.AddMessageFilter(Me)
    End Sub

    Private Sub OnParentDeactivate(ByVal sender As Object, ByVal e As EventArgs) Handles parent.Deactivate
        Application.RemoveMessageFilter(Me)
    End Sub

    Public Function PreFilterMessage(ByRef m As Message) As Boolean Implements IMessageFilter.PreFilterMessage
        Select Case m.Msg
            Case WM_MOUSEWHEEL
                ' don't handle all (e.g. Ctrl-MouseWheel: zoom feature in IE)
                If Not Control.ModifierKeys = Keys.None Then
                    Return False
                End If

                ' get position (better debug support than calling Control.MousePosition in GetTopmostChild):
                Dim screenPoint As New Point(m.LParam.ToInt32())
                ' redirect the wheel message to the topmost child control
                Dim child As Control = GetTopmostChild(parent, screenPoint)

                If child IsNot Nothing Then
                    If m.HWnd = child.Handle AndAlso child.Focused Then
                        Return False    ' control is focused, so it should handle the wheel itself
                    End If
                    ' thanks to http://sourceforge.net/users/kevindente/:
                    If TypeOf child Is WebBrowser Then
                        Return ScrollHtmlControl(TryCast(child, WebBrowser), m)
                    End If

                    If Not m.HWnd = child.Handle Then
                        ' no recursion, please. Redirect message...
                        PostMessage(child.Handle, WM_MOUSEWHEEL, m.WParam, m.LParam)
                        Return True
                    End If

                    Return False
                End If
        End Select
        Return False
    End Function

    Public Function GetTopmostChild(ByVal ctrl As Control, ByVal mousePosition As Point) As Control
        If ctrl.Controls.Count > 0 Then
            Dim p As Point = ctrl.PointToClient(mousePosition)
            Dim child As Control = ctrl.GetChildAtPoint(p, GetChildAtPointSkip.Invisible)
            If child IsNot Nothing Then
                Return GetTopmostChild(child, mousePosition)
            Else
                Return ctrl
            End If
        Else
            Return ctrl
        End If
    End Function

    Private Function ScrollHtmlControl(ByVal wbc As WebBrowser, ByVal m As Message) As Boolean
        Dim hwnd As IntPtr

        Dim control As IWebBrowser2 = CType(wbc.ActiveXInstance, IWebBrowser2)

        Dim oleWindow As IOleWindow = Nothing
        Try
            oleWindow = TryCast(control.Document, IOleWindow)
        Catch
        End Try
        If oleWindow Is Nothing Then Return False

        oleWindow.GetWindow(hwnd)
        If m.HWnd = hwnd Then
            ' avoid recursion
            Return False
        End If

        PostMessage(hwnd, WM_MOUSEWHEEL, m.WParam, m.LParam)

        Return True
    End Function

#Region " Win32 interop/helpers "

    <DllImport("user32.dll")> _
    Private Shared Function PostMessage(ByVal hWnd As IntPtr, ByVal wMsg As Integer, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As Boolean
    End Function

    Private Const WM_MOUSEWHEEL As Integer = &H20A

#End Region

End Class
