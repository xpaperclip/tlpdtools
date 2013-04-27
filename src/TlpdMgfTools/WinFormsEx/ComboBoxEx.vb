Imports System
Imports System.Drawing
Imports System.Windows.Forms

Public NotInheritable Class ComboBoxEx : Inherits ComboBox
    Public Sub New()
        DrawMode = DrawMode.OwnerDrawFixed
        DropDownStyle = ComboBoxStyle.DropDownList
    End Sub

    Protected Overrides Sub OnDrawItem(e As DrawItemEventArgs)
        e.DrawBackground()
        e.DrawFocusRectangle()

        If e.Index >= 0 AndAlso e.Index < Items.Count Then
            Dim item As DropDownItem = CType(Items(e.Index), DropDownItem)
            Dim stringFormat As New StringFormat()
            stringFormat.Alignment = StringAlignment.Near
            stringFormat.LineAlignment = StringAlignment.Center

            e.Graphics.DrawImage(item.Image, e.Bounds.Left + 2, e.Bounds.Top + CInt(Math.Floor((e.Bounds.Height - item.Image.Height) / 2)))
            Using brush As New SolidBrush(e.ForeColor)
                e.Graphics.DrawString(item.Value, e.Font, brush,
                                      New RectangleF(e.Bounds.Left + item.Image.Width + 3, e.Bounds.Top, e.Bounds.Width - item.Image.Width - 3, e.Bounds.Height), stringFormat)
            End Using
        End If

        MyBase.OnDrawItem(e)
    End Sub

    Protected Overrides Sub OnMeasureItem(e As MeasureItemEventArgs)
        MyBase.OnMeasureItem(e)
        If e.Index >= 0 AndAlso e.Index < Items.Count Then
            Dim item As DropDownItem = CType(Items(e.Index), DropDownItem)
            e.ItemHeight = Math.Max(e.ItemHeight, item.Image.Height)
            e.ItemWidth = e.ItemWidth + item.Image.Width + 3
        End If
    End Sub
End Class

Public NotInheritable Class DropDownItem

    Public Property Value As String
    Public Property Image As Image

    Public Sub New()
        Me.New("", Color.Empty)
    End Sub
    Public Sub New(val As String, image As Image)
        Me.Value = val
        Me.Image = image
    End Sub
    Public Sub New(val As String, color As Color)
        Me.Value = val
        Me.Image = New Bitmap(20, 20)
        Using g As Graphics = Graphics.FromImage(Image)
            g.Clear(Drawing.Color.Transparent)
            Using b As Brush = New SolidBrush(color)
                g.FillRectangle(b, 0, 0, Image.Width, Image.Height)
            End Using
        End Using
    End Sub

    Public Overrides Function ToString() As String
        Return Value
    End Function

End Class
