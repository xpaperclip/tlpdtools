Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Windows.Forms
Imports System.Xml.Linq
Imports FastColoredTextBoxNS

Class MainForm
    Inherits Form

    Sub New()
        InitializeComponent()
    End Sub

    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        cmbTlpdDatabase.Items.Add(New TlpdDatabase("SC2 Heart of the Swarm", "hots", "sc2-hots"))
        cmbTlpdDatabase.Items.Add(New TlpdDatabase("SC2 WoL Korean", "sc2-korean", "sc2-korean"))
        cmbTlpdDatabase.Items.Add(New TlpdDatabase("SC2 WoL International", "sc2-international", "sc2-intl"))
        cmbTlpdDatabase.Items.Add(New TlpdDatabase("BW Korean SOSPA", "sospa", "bw-sospa"))
        cmbTlpdDatabase.Items.Add(New TlpdDatabase("BW Korean Pro", "korean", "bw-korean"))
        cmbTlpdDatabase.Items.Add(New TlpdDatabase("BW International", "international", "bw-intl"))
        cmbTlpdDatabase.SelectedIndex = 0

        If File.Exists("README.mgf") Then
            _initialTab = NewTab(DocumentScheme.Mgf, File.ReadAllText("README.mgf"), "README")
            _untitledCount = 0
        Else
            _initialTab = NewTab(DocumentScheme.Mgf, "", "Untitled")
        End If
    End Sub
    Private Sub MainForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        For Each tab As DocumentTabPage In tabControl.TabPages
            If Not CloseTab(tab) Then
                e.Cancel = True
            End If
        Next
    End Sub

    Private ReadOnly Property CurrentTab As DocumentTabPage
        Get
            Return CType(tabControl.SelectedTab, DocumentTabPage)
        End Get
    End Property
    Private _initialTab As DocumentTabPage
    Private _untitledCount As Integer = 1

    Private fileFilter As String = "Tool formats (*.mgf, *.tlpdcode, *.wiki)|*.mgf;*.tlpdcode;*.wiki|MGFormatter (*.mgf)|*.mgf|Wikicode (*.wiki)|*.wiki|TLPD Import Code (*.tlpdcode)|*.tlpdcode|All Files (*.*)|*.*"

#Region " File Menu "

    Private Sub NewToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles NewToolStripMenuItem.Click, btnNew.Click, Me.DoubleClick
        _initialTab = Nothing
        _untitledCount += 1
        NewTab(DocumentScheme.Mgf, "", "Untitled" + If(_untitledCount > 1, " (" + _untitledCount.ToString() + ")", ""))
    End Sub
    Private Sub OpenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles OpenToolStripMenuItem.Click, btnOpen.Click
        Using dlg As New OpenFileDialog()
            dlg.Filter = fileFilter
            If dlg.ShowDialog() = Windows.Forms.DialogResult.OK Then
                OpenTab(dlg.FileName)
            End If
        End Using
    End Sub
    Private Sub OpenLiquipediaBWPageMenuItem_Click(sender As Object, e As EventArgs) Handles OpenLiquipediaBWPageMenuItem.Click
        Dim page As String = Nothing
        If UI.InputBox(Me, "Open Liquipedia Page", "Page", page) = Windows.Forms.DialogResult.OK Then
            OpenLpPage(page, "starcraft")
        End If
    End Sub
    Private Sub OpenLiquipediaPageToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles OpenLiquipediaPageToolStripMenuItem.Click
        Dim page As String = Nothing
        If UI.InputBox(Me, "Open Liquipedia Page", "Page", page) = Windows.Forms.DialogResult.OK Then
            OpenLpPage(page, "starcraft2")
        End If
    End Sub
    Private Sub CloseToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CloseToolStripMenuItem.Click
        CloseTab(CurrentTab)
    End Sub
    Private Sub CloseAllToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CloseAllToolStripMenuItem.Click, CloseAllToolStripContextMenuItem.Click
        For Each tab As DocumentTabPage In tabControl.TabPages
            If Not CloseTab(tab) Then Exit For
        Next
    End Sub
    Private Sub SaveToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SaveToolStripMenuItem.Click, btnSave.Click
        SaveTab(CurrentTab)
    End Sub
    Private Sub SaveAsToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SaveAsToolStripMenuItem.Click
        SaveAsTab(CurrentTab)
    End Sub
    Private Sub SaveAllToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SaveAllToolStripMenuItem.Click
        For Each tab As DocumentTabPage In tabControl.TabPages
            If Not SaveTab(tab) Then Exit For
        Next
    End Sub
    Private Sub ExitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExitToolStripMenuItem.Click
        Application.Exit()
    End Sub

#End Region

#Region " Edit Menu "

    Private Sub EditToolStripMenuItem_DropDownOpening(sender As Object, e As EventArgs) Handles EditToolStripMenuItem.DropDownOpening
        UndoToolStripMenuItem.Enabled = If(CurrentTab Is Nothing, False, CurrentTab.tb.UndoEnabled)
        RedoToolStripMenuItem.Enabled = If(CurrentTab Is Nothing, False, CurrentTab.tb.RedoEnabled)

        CutToolStripMenuItem.Enabled = If(CurrentTab Is Nothing, False, True)
        CopyToolStripMenuItem.Enabled = If(CurrentTab Is Nothing, False, True)
        PasteToolStripMenuItem.Enabled = If(CurrentTab Is Nothing, False, True)

        SelectAllToolStripMenuItem.Enabled = If(CurrentTab Is Nothing, False, True)
        GotoToolStripMenuItem.Enabled = If(CurrentTab Is Nothing, False, True)

        FindToolStripMenuItem.Enabled = If(CurrentTab Is Nothing, False, True)
        ReplaceToolStripMenuItem.Enabled = If(CurrentTab Is Nothing, False, True)

        TLPDizeToolStripMenuItem.Enabled = If(CurrentTab Is Nothing, False, CurrentTab.tb.SelectionLength > 0)
    End Sub
    Private Sub UndoToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles UndoToolStripMenuItem.Click
        If CurrentTab Is Nothing Then Return
        CurrentTab.tb.Undo()
    End Sub
    Private Sub RedoToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RedoToolStripMenuItem.Click
        If CurrentTab Is Nothing Then Return
        CurrentTab.tb.Redo()
    End Sub
    Private Sub CutToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CutToolStripMenuItem.Click, CutToolStripContextMenuItem.Click
        If CurrentTab Is Nothing Then Return
        CurrentTab.tb.Cut()
    End Sub
    Private Sub CopyToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CopyToolStripMenuItem.Click, CopyToolStripContextMenuItem.Click
        If CurrentTab Is Nothing Then Return
        CurrentTab.tb.Copy()
    End Sub
    Private Sub PasteToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles PasteToolStripMenuItem.Click, PasteToolStripContextMenuItem.Click
        If CurrentTab Is Nothing Then Return
        CurrentTab.tb.Paste()
    End Sub
    Private Sub SelectAllToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SelectAllToolStripMenuItem.Click, SelectAllToolStripContextMenuItem.Click
        If CurrentTab Is Nothing Then Return
        CurrentTab.tb.SelectAll()
    End Sub
    Private Sub GotoToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles GotoToolStripMenuItem.Click
        If CurrentTab Is Nothing Then Return
        CurrentTab.tb.ShowGoToDialog()
    End Sub
    Private Sub FindToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles FindToolStripMenuItem.Click
        If CurrentTab Is Nothing Then Return
        CurrentTab.tb.ShowFindDialog()
    End Sub
    Private Sub ReplaceToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ReplaceToolStripMenuItem.Click
        If CurrentTab Is Nothing Then Return
        CurrentTab.tb.ShowReplaceDialog()
    End Sub
    Private Sub TLPDizeToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles TLPDizeToolStripMenuItem.Click, TLPDizeToolStripContextMenuItem.Click, btnTLPDize.Click
        Dim request = Tlpd.GetRequest(CurrentTab.tb.SelectedText, CType(cmbTlpdDatabase.SelectedItem, TlpdDatabase))
        Using frm As New DownloadProgressForm(request)
            frm.Image = EmbeddedResources.tlpd
            If frm.ShowDialog(Me) = Windows.Forms.DialogResult.OK Then
                CurrentTab.tb.SelectedText = frm.Result
            End If
        End Using
    End Sub

#End Region

#Region " Run Menu "

    Private Sub ProcessToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ProcessToolStripMenuItem.Click, btnProcess.Click
        If CurrentTab Is Nothing Then Return

        _initialTab = Nothing
        Dim data As String = CurrentTab.Process()
        Select Case CurrentTab.Scheme
            Case DocumentScheme.Lp
                NewTab(DocumentScheme.Mgf, data, CurrentTab.BaseText + ".mgf")
            Case DocumentScheme.Mgf
                NewTab(DocumentScheme.Tlpd, data, CurrentTab.BaseText + " [Output]")
        End Select
    End Sub
    Private Sub ProcessSelectionToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ProcessSelectionToolStripMenuItem.Click
        If CurrentTab Is Nothing Then Return

        _initialTab = Nothing
        Dim data As String = CurrentTab.Process(True)
        Select Case CurrentTab.Scheme
            Case DocumentScheme.Lp
                NewTab(DocumentScheme.Mgf, data, CurrentTab.BaseText + " [Output]")
        End Select
    End Sub

#End Region

#Region " Quick Links Menu "

    Private Sub TLPDStaffForumToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles TLPDStaffForumToolStripMenuItem.Click
        System.Diagnostics.Process.Start("http://www.teamliquid.net/forum/index.php?show_part=47")
    End Sub
    Private Sub DoEloCalculationsToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DoEloCalculationsToolStripMenuItem.Click
        System.Diagnostics.Process.Start("http://www.teamliquid.net/tlpd/admin/game_edit.php?action=do_elo")
    End Sub

#End Region

#Region " Window Menu "

    Private Sub AlwaysOnTopToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AlwaysOnTopToolStripMenuItem.Click
        AlwaysOnTopToolStripMenuItem.Checked = Not AlwaysOnTopToolStripMenuItem.Checked
        Me.TopMost = AlwaysOnTopToolStripMenuItem.Checked
    End Sub
    Private Sub OpacityToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles OpacityToolStripMenuItem.Click
        OpacityToolStripMenuItem.Checked = Not OpacityToolStripMenuItem.Checked
        If OpacityToolStripMenuItem.Checked Then
            Me.Opacity = 0.5
        Else
            Me.Opacity = 1
        End If
    End Sub

#End Region

#Region " Help Menu "

    Private Sub GithubToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles GithubToolStripMenuItem.Click
        Diagnostics.Process.Start("http://www.github.com/xpaperclip/tlpdtools")
    End Sub
    Private Sub AboutToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AboutToolStripMenuItem.Click
        Using frm As New AboutForm()
            frm.ShowDialog(Me)
        End Using
    End Sub

#End Region

#Region " Context Menu "

    Private Sub Open0ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles Open0ToolStripContextMenuItem.Click
        Dim tag As String = mnuContext.Tag.ToString()
        OpenTab(tag)
    End Sub
    Private Sub OpenLinkToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles OpenLinkToolStripContextMenuItem.Click
        Dim tag As String = mnuContext.Tag.ToString()
        Diagnostics.Process.Start(mnuContext.Tag.ToString())
    End Sub
    Private Sub CopyLinkToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CopyLinkToolStripContextMenuItem.Click
        Dim tag As String = mnuContext.Tag.ToString()
        Clipboard.SetDataObject(tag)
    End Sub

#End Region

#Region " Tab Context Menu "

    Private Sub mnuTabContext_Opening(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles mnuTabContext.Opening
        Dim tab As DocumentTabPage = CType(mnuTabContext.Tag, DocumentTabPage)

        If String.IsNullOrEmpty(tab.Filename) Then
            SaveToolStripContextMenuItem.Text = "Save Tab"
            OpenContainingFolderToolStripContextMenuItem.Enabled = False
        Else
            SaveToolStripContextMenuItem.Text = "Save " + Path.GetFileName(tab.Filename)
            OpenContainingFolderToolStripContextMenuItem.Enabled = True
        End If
    End Sub
    Private Sub SaveToolStripContextMenuItem_Click(sender As Object, e As EventArgs) Handles SaveToolStripContextMenuItem.Click
        Dim tab As DocumentTabPage = CType(mnuTabContext.Tag, DocumentTabPage)
        SaveTab(tab)
    End Sub
    Private Sub CloseTabToolStripContextMenuItem_Click(sender As Object, e As EventArgs) Handles CloseTabToolStripContextMenuItem.Click
        Dim tab As DocumentTabPage = CType(mnuTabContext.Tag, DocumentTabPage)
        CloseTab(tab)
    End Sub
    Private Sub CloseAllButThisToolStripContextMenuItem_Click(sender As Object, e As EventArgs) Handles CloseAllButThisToolStripContextMenuItem.Click
        Dim tab As DocumentTabPage = CType(mnuTabContext.Tag, DocumentTabPage)
        For Each othertab As DocumentTabPage In tabControl.TabPages
            If Not othertab Is tab Then
                If Not CloseTab(othertab) Then Exit For
            End If
        Next
    End Sub
    Private Sub OpenContainingFolderToolStripContextMenuItem_Click(sender As Object, e As EventArgs) Handles OpenContainingFolderToolStripContextMenuItem.Click
        Dim tab As DocumentTabPage = CType(mnuTabContext.Tag, DocumentTabPage)
        If String.IsNullOrEmpty(tab.Filename) Then Return

        Diagnostics.Process.Start("explorer.exe", "/select,""" + tab.Filename + """")
    End Sub

#End Region

    Private Sub cmbScheme_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbScheme.SelectedIndexChanged
        Select Case cmbScheme.SelectedIndex
            Case 0 : CurrentTab.Scheme = DocumentScheme.Mgf
            Case 1 : CurrentTab.Scheme = DocumentScheme.Tlpd
            Case 2 : CurrentTab.Scheme = DocumentScheme.Lp
        End Select
        DoUI(False)
    End Sub

    Private Sub ctl_DragEnter(sender As Object, e As DragEventArgs) Handles tabControl.DragEnter, mnuMenu.DragEnter, tlsToolbar.DragEnter
        If e.Data.GetDataPresent(DataFormats.Text) Then
            Dim text As String = e.Data.GetData(DataFormats.Text).ToString()
            If Not TryGetLpPage(text) Is Nothing Then
                e.Effect = DragDropEffects.Link
            Else
                e.Effect = DragDropEffects.None
            End If
        ElseIf e.Data.GetDataPresent(DataFormats.FileDrop) Then
            e.Effect = DragDropEffects.Copy
        Else
            e.Effect = DragDropEffects.None
        End If
    End Sub
    Private Sub ctl_DragDrop(sender As Object, e As DragEventArgs) Handles tabControl.DragDrop, mnuMenu.DragDrop, tlsToolbar.DragDrop
        If e.Data.GetDataPresent(DataFormats.Text) Then
            Dim text As String = e.Data.GetData(DataFormats.Text).ToString()
            Dim lp As String = Nothing
            Dim lp_page As String = TryGetLpPage(text, lp)
            If Not lp_page Is Nothing Then
                OpenLpPage(lp_page, lp)
            End If
        ElseIf e.Data.GetDataPresent(DataFormats.FileDrop) Then
            Dim files As String() = CType(e.Data.GetData(DataFormats.FileDrop), String())
            Dim sb As New System.Text.StringBuilder()
            sb.AppendLine("; Dropped the following files... can't do anything with them yet.")
            Dim cantuse As Boolean = False
            For Each file As String In files
                Dim ext As String = Path.GetExtension(file).ToLower()
                If ext = ".mgf" OrElse ext = ".wiki" OrElse ext = ".tlpdcode" Then
                    OpenTab(file)
                Else
                    cantuse = True
                    sb.AppendLine("; " + file)
                End If
            Next
            If cantuse Then
                NewTab(DocumentScheme.Mgf, sb.ToString(), "Dropped Files")
            End If
        End If
    End Sub

    Private Sub tabControl_Selected(sender As Object, e As TabControlEventArgs) Handles tabControl.Selected
        DoUI()
    End Sub
    Private Sub tabControl_MouseClick(sender As Object, e As MouseEventArgs) Handles tabControl.MouseClick
        Dim tab As DocumentTabPage = Nothing
        For i As Integer = 0 To tabControl.TabPages.Count - 1
            If tabControl.GetTabRect(i).Contains(e.Location) Then
                tab = CType(tabControl.TabPages(i), DocumentTabPage)
                Exit For
            End If
        Next
        If tab Is Nothing Then Return

        Select Case e.Button
            Case Windows.Forms.MouseButtons.Right
                mnuTabContext.Tag = tab
                mnuTabContext.Show(tabControl, e.Location)
            Case Windows.Forms.MouseButtons.Middle
                CloseTab(tab)
        End Select
    End Sub

    Private Sub DoUI()
        DoUI(True)
    End Sub
    Private Sub DoUI(doSchemes As Boolean)
        If CurrentTab Is Nothing Then
            CloseToolStripMenuItem.Enabled = False
            SaveToolStripMenuItem.Enabled = False
            SaveAsToolStripMenuItem.Enabled = False
            btnSave.Enabled = False
            cmbScheme.Enabled = False
            btnTLPDize.Enabled = False
            ProcessToolStripMenuItem.Enabled = False
            ProcessSelectionToolStripMenuItem.Enabled = False
            btnProcess.Enabled = False
            Return
        Else
            CloseToolStripMenuItem.Enabled = True
            SaveToolStripMenuItem.Enabled = True
            SaveAsToolStripMenuItem.Enabled = True
            btnSave.Enabled = True
            cmbScheme.Enabled = True
            btnTLPDize.Enabled = True
            ProcessToolStripMenuItem.Enabled = True

            Select Case CurrentTab.Scheme
                Case DocumentScheme.Mgf
                    btnProcess.Enabled = True
                    ProcessSelectionToolStripMenuItem.Enabled = False
                Case DocumentScheme.Tlpd
                    btnProcess.Enabled = False
                    ProcessSelectionToolStripMenuItem.Enabled = False
                Case DocumentScheme.Lp
                    btnProcess.Enabled = True
                    ProcessSelectionToolStripMenuItem.Enabled = True
            End Select
            ProcessToolStripMenuItem.Enabled = btnProcess.Visible

            If doSchemes Then
                Select Case CurrentTab.Scheme
                    Case DocumentScheme.Mgf
                        cmbScheme.SelectedIndex = 0
                    Case DocumentScheme.Tlpd
                        cmbScheme.SelectedIndex = 1
                    Case DocumentScheme.Lp
                        cmbScheme.SelectedIndex = 2
                End Select
            End If
        End If
    End Sub

    Public Function NewTab(scheme As DocumentScheme, initialData As String, title As String) As DocumentTabPage
        Dim tab As New DocumentTabPage(Me)
        tab.Scheme = scheme
        tab.tb.Text = initialData
        tab.Text = title
        tab.tb.ClearUndo()
        tab.tb.IsChanged = False
        tabControl.TabPages.Add(tab)
        If CurrentTab Is _initialTab AndAlso CurrentTab.Filename Is Nothing AndAlso Not CurrentTab.tb.IsChanged Then
            CloseTab(CurrentTab)
        End If
        tabControl.SelectedTab = tab
        DoUI(True)
        Return tab
    End Function
    Private Sub OpenTab(filename As String)
        Using sr As New StreamReader(filename)
            Dim tab As DocumentTabPage
            Dim scheme As DocumentScheme
            Select Case Path.GetExtension(filename)
                Case ".mgf"
                    scheme = DocumentScheme.Mgf
                Case ".tlpdcode"
                    scheme = DocumentScheme.Tlpd
                Case ".wiki"
                    scheme = DocumentScheme.Lp
            End Select
            tab = NewTab(scheme, sr.ReadToEnd(), Path.GetFileName(filename))
            tab.Filename = filename
        End Using
    End Sub
    Private Function SaveTab(tab As DocumentTabPage) As Boolean
        If tab Is Nothing Then Return True
        If tab.Filename Is Nothing Then
            Return SaveAsTab(tab)
        Else
            File.WriteAllText(tab.Filename, tab.tb.Text)
            tab.tb.IsChanged = False
            Return True
        End If
    End Function
    Private Function SaveAsTab(tab As DocumentTabPage) As Boolean
        If tab Is Nothing Then Return True
        _initialTab = Nothing
        Using dlg As New SaveFileDialog
            dlg.Filter = fileFilter
            If tab.Filename Is Nothing Then
                dlg.FileName = tab.BaseText
            Else
                Try
                    dlg.FileName = Path.GetFileName(tab.Filename)
                    dlg.InitialDirectory = Path.GetDirectoryName(tab.Filename)
                Catch
                End Try
            End If

            Select Case tab.Scheme
                Case DocumentScheme.Mgf : dlg.FilterIndex = 2
                Case DocumentScheme.Lp : dlg.FilterIndex = 3
                Case DocumentScheme.Tlpd : dlg.FilterIndex = 4
            End Select

            If dlg.ShowDialog = Windows.Forms.DialogResult.OK Then
                tab.Filename = dlg.FileName
                tab.Text = Path.GetFileName(dlg.FileName)

                File.WriteAllText(tab.Filename, tab.tb.Text)
                tab.tb.IsChanged = False
                Return True
            Else
                Return False
            End If
        End Using
    End Function
    Private Function CloseTab(tab As DocumentTabPage) As Boolean
        If tab Is Nothing Then Return True

        _initialTab = Nothing

        If tab.tb.IsChanged Then
            tabControl.SelectedTab = tab
            Select Case MessageBox.Show("Do you want to save changes before closing this tab?", tab.BaseText, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)
                Case Windows.Forms.DialogResult.Yes
                    SaveTab(tab)
                    If Not CurrentTab.tb.IsChanged Then
                        tabControl.TabPages.Remove(tab)
                    End If
                Case Windows.Forms.DialogResult.No
                    tabControl.TabPages.Remove(tab)
                Case Windows.Forms.DialogResult.Cancel
                    Return False
            End Select
        Else
            tabControl.TabPages.Remove(tab)
        End If
        Return True
    End Function

    Private Sub OpenLpPage(page As String)
        OpenLpPage(page, "starcraft2")
    End Sub
    Private Sub OpenLpPage(page As String, lp As String)
        UseWaitCursor = True

        If lp Is Nothing Then
            lp = "starcraft2"
        End If

        If page.StartsWith("http://wiki.teamliquid.net/" + lp + "/index.php?title=") AndAlso page.Contains("action=edit") Then
            Using frm = DownloadProgressForm.BeginDownload(page)
                If frm.ShowDialog(Me) = Windows.Forms.DialogResult.OK Then
                    Dim text As String = frm.Result
                    text = text.TrimBetween("<textarea", "</textarea>").From(">").Replace("&lt;", "<").Replace("&amp;", "&")
                    NewTab(DocumentScheme.Lp, text, page.TrimBetween("title=", "&") + " [LP]")
                End If
            End Using
        Else
            Dim url As String = "http://wiki.teamliquid.net/" + lp + "/api.php?format=xml&action=query&titles=" + _
                Uri.EscapeUriString(page.Replace("%20", " ").Replace(" ", "_")) + "&prop=revisions&rvprop=content"
            Using frm = DownloadProgressForm.BeginDownload(url)
                If frm.ShowDialog(Me) = Windows.Forms.DialogResult.OK Then
                    Dim text As String = frm.Result

                    Dim xml As XDocument = XDocument.Parse(text)
                    Dim xpage As XElement = xml.<api>.<query>.<pages>.<page>.First()
                    Dim data As String = xpage.<revisions>.<rev>.Value()
                    NewTab(DocumentScheme.Lp, data, xpage.@title + " [LP]")
                End If
            End Using
        End If

        UseWaitCursor = False
    End Sub
    Private Function TryGetLpPage(url As String) As String
        Dim lp As String = Nothing
        Return TryGetLpPage(url, lp)
    End Function
    Private Function TryGetLpPage(url As String, ByRef lp As String) As String
        Try
            If url.StartsWith("http://wiki.teamliquid.net/starcraft2/index.php?title=") AndAlso url.Contains("action=edit") Then
                lp = "starcraft2"
                Return url
            End If

            If url.StartsWith("http://wiki.teamliquid.net/starcraft/index.php?title=") AndAlso url.Contains("action=edit") Then
                lp = "starcraft"
                Return url
            End If

            Dim uri As New Uri(url)
            If uri.Host.ToLower() = "wiki.teamliquid.net" Then
                If uri.AbsolutePath.StartsWith("/starcraft2/") Then
                    lp = "starcraft2"
                    Return uri.AbsolutePath.Substring("/starcraft2/".Length)
                End If
                If uri.AbsolutePath.StartsWith("/starcraft/") Then
                    lp = "starcraft"
                    Return uri.AbsolutePath.Substring("/starcraft/".Length)
                End If
            End If
        Catch ex As Exception
        End Try
        Return Nothing
    End Function

End Class
