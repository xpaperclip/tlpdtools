Imports System
Imports System.Drawing
Imports System.Windows.Forms
Imports FastColoredTextBoxNS

Enum DocumentScheme
    Mgf
    Tlpd
    Lp
End Enum

Class DocumentTabPage
    Inherits TabPage

    Public Sub New(frm As MainForm)
        MyBase.New()
        Me.frm = frm
        Me.Font = frm.Font

        Me.Padding = New System.Windows.Forms.Padding(3)
        Me.Text = "Untitled"
        Me.UseVisualStyleBackColor = True

        tb = New FastColoredTextBox()
        tb.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        tb.CurrentLineColor = Color.FromArgb(218, 218, 229) 'SystemColors.ActiveCaption
        tb.Font = New System.Drawing.Font("Courier New", 12.75!)
        tb.LeftPadding = 12
        tb.ReservedCountOfLineNumberChars = 2
        tb.SelectionColor = SystemColors.Highlight
        tb.Dock = System.Windows.Forms.DockStyle.Fill
        tb.HotkeysMapping.Remove(Keys.Alt Or Keys.F)
        tb.AutoIndent = False

        autocomplete = New FastColoredTextBoxNS.AutocompleteMenu(tb)
        autocomplete.AppearInterval = 200
        autocomplete.MinFragmentLength = 0
        autocomplete.AllowTabKey = True
        autocomplete.Items.Font = Me.Font
        autocomplete.Items.ImageList = frm.iglAutocomplete
        autocomplete.Items.MaximumSize = New Size(200, 300)
        autocomplete.Items.Width = 200
        autocomplete.Items.HighlightColor = ProfessionalColors.MenuItemSelectedGradientEnd
        autocomplete.Items.HighlightBorderPen = New Pen(ProfessionalColors.MenuItemSelected)

        Me.Controls.Add(tb)
    End Sub

    Private _dirty As Boolean
    Private _text As String
    Public ReadOnly Property BaseText As String
        Get
            Return _text
        End Get
    End Property
    Public Overrides Property Text As String
        Get
            Return _text + If(_dirty, "*", "")
        End Get
        Set(Value As String)
            _text = Value
            MyBase.Text = _text + If(_dirty, "*", "")
        End Set
    End Property

    Friend frm As MainForm
    Friend WithEvents tb As FastColoredTextBox
    Friend WithEvents autocomplete As AutocompleteMenu
    Private mgfService As MgfService

    Private _scheme As DocumentScheme
    Public Property Scheme As DocumentScheme
        Get
            Return _scheme
        End Get
        Set(Value As DocumentScheme)
            _scheme = Value
            tb.DescriptionFile = ""
            tb.ClearStylesBuffer()
            Select Case _scheme
                Case DocumentScheme.Mgf
                    mgfService = New MgfService(tb)
                Case DocumentScheme.Tlpd
                    tb.DescriptionFile = "tlpdFormat.xml"
                Case DocumentScheme.Lp
                    tb.DescriptionFile = "wikiFormat.xml"
            End Select
            Dim isChanged As Boolean = tb.IsChanged
            tb.OnTextChanged()
            tb.IsChanged = isChanged
        End Set
    End Property

    Private _filename As String
    Public Property Filename As String
        Get
            Return _filename
        End Get
        Set(Value As String)
            _filename = Value
            If Not mgfService Is Nothing Then
                mgfService.Filename = _filename
            End If
        End Set
    End Property

    Private Sub tb_DirtyStatusChanged(sender As Object, e As EventArgs) Handles tb.DirtyStatusChanged
        _dirty = tb.IsChanged
        Me.Text = _text
    End Sub

    Private Sub tb_KeyUp(sender As Object, e As KeyEventArgs) Handles tb.KeyUp
        If e.KeyData = (Keys.Space Or Keys.Control) Then
            autocomplete.Show(True)
            e.Handled = True
        End If
    End Sub

    Private Sub tb_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles tb.MouseDoubleClick
        ' -- turns out this is pretty annoying when editing
        'If mgfService Is Nothing Then Return
        'Dim p As Place = tb.PointToPlace(e.Location)
        'If mgfService.CharIsHyperlink(p) Then
        '    Dim url As String = mgfService.GetLinkUrl(p, CType(frm.cmbTlpdDatabase.SelectedItem, TlpdDatabase))
        '    If Not url.StartsWith("$") Then
        '        Diagnostics.Process.Start(url)
        '    End If
        '    Exit Sub
        'End If
    End Sub
    Private Sub tb_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles tb.MouseMove
        If mgfService Is Nothing Then Return
        Dim p As Place = tb.PointToPlace(e.Location)
        If mgfService.CharIsHyperlink(p) Then
            tb.Cursor = Cursors.Hand
        Else
            tb.Cursor = Cursors.IBeam
        End If
    End Sub
    Private Sub tb_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles tb.MouseUp
        Dim p As Place = tb.PointToPlace(e.Location)
        Dim contextMenuLink As Boolean = False

        If Not mgfService Is Nothing Then
            If mgfService.CharIsHyperlink(p) Then
                Dim url As String = mgfService.GetLinkUrl(p, CType(frm.cmbTlpdDatabase.SelectedItem, TlpdDatabase))
                Select Case e.Button
                    Case Windows.Forms.MouseButtons.Left
                        'do nothing
                    Case Windows.Forms.MouseButtons.Right
                        frm.mnuContext.Tag = url
                        contextMenuLink = True
                End Select
            End If
        End If

        frm.LinkContextMenuSeparator.Visible = contextMenuLink
        frm.OpenLinkToolStripMenuItem.Visible = contextMenuLink
        frm.CopyLinkToolStripMenuItem.Visible = contextMenuLink

        Select Case e.Button
            Case Windows.Forms.MouseButtons.Right
                frm.TLPDizeToolStripContextMenuItem.Enabled = (tb.SelectedText.Length > 0)
                frm.mnuContext.Show(tb, e.Location)
        End Select
    End Sub

    Private Sub tb_TextChanged(ByVal sender As Object, ByVal e As FastColoredTextBoxNS.TextChangedEventArgs) Handles tb.TextChanged
        If Me.Scheme = DocumentScheme.Mgf And Not mgfService Is Nothing Then
            mgfService.Colourize()
        End If
    End Sub
    Private Sub tb_TextChangedDelayed(ByVal sender As Object, ByVal e As FastColoredTextBoxNS.TextChangedEventArgs) Handles tb.TextChangedDelayed
        e.ChangedRange.ClearFoldingMarkers()
        If Me.Scheme = DocumentScheme.Lp Then
            e.ChangedRange.SetFoldingMarkers("{{", "}}")
        End If
        If Me.Scheme = DocumentScheme.Mgf Then
            If Not mgfService Is Nothing Then
                mgfService.Colourize(True)
            End If
        End If
    End Sub
    Private Sub autocomplete_AutocompleteNeeded(sender As Object, e As AutocompleteNeededEventArgs) Handles autocomplete.AutocompleteNeeded
        If Me.Scheme = DocumentScheme.Mgf And Not mgfService Is Nothing Then
            mgfService.AutocompleteNeeded(autocomplete, e)
        End If
    End Sub

    Public Function Process() As String
        Return Process(False)
    End Function
    Public Function Process(selection As Boolean) As String
        If Me.Scheme = DocumentScheme.Mgf And Not mgfService Is Nothing Then
            Return mgfService.Process()
        End If
        If Me.Scheme = DocumentScheme.Lp Then
            Dim lpProcessor As New LpProcessor()
            Return lpProcessor.Process(If(selection, tb.SelectedText, tb.Text))
        End If
        Return "Service unavailable."
    End Function

End Class
