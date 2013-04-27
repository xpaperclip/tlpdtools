Imports System
Imports System.Reflection
Imports System.Security
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices

<ComImport(), Guid("D30C1661-CDAF-11D0-8A3E-00C04FC9E26E"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch), DefaultMember("Name"), SuppressUnmanagedCodeSecurity()> _
Public Interface IWebBrowser2
    ' Methods
    <DispId(301)> _
    Sub ClientToWindow(<[In](), Out()> ByRef pcx As Integer, <[In](), Out()> ByRef pcy As Integer)
    <DispId(502)> _
    Sub ExecWB(ByVal cmdID As OLECMDID, ByVal cmdexecopt As OLECMDEXECOPT, <[In]()> ByRef pvaIn As Object, <[In](), Out()> ByRef pvaOut As Object)
    <DispId(303)> _
    Function GetProperty(<MarshalAs(UnmanagedType.BStr)> ByVal [Property] As String) As Object
    <DispId(100)> _
    Sub GoBack()
    <DispId(101)> _
    Sub GoForward()
    <DispId(102)> _
    Sub GoHome()
    <DispId(103)> _
    Sub GoSearch()
    <DispId(104)> _
    Sub Navigate(<MarshalAs(UnmanagedType.BStr)> ByVal URL As String, <[In]()> ByRef Flags As Object, <[In]()> ByRef TargetFrameName As Object, <[In]()> ByRef PostData As Object, <[In]()> ByRef Headers As Object)
    <DispId(500)> _
    Sub Navigate2(<[In]()> ByRef URL As Object, <[In]()> ByRef Flags As Object, <[In]()> ByRef TargetFrameName As Object, <[In]()> ByRef PostData As Object, <[In]()> ByRef Headers As Object)
    <DispId(302)> _
    Sub PutProperty(<MarshalAs(UnmanagedType.BStr)> ByVal [Property] As String, ByVal vtValue As Object)
    <DispId(501)> _
    Function QueryStatusWB(ByVal cmdID As OLECMDID) As OLECMDF
    <DispId(300)> _
    Sub Quit()
    <DispId(-550)> _
    Sub Refresh()
    <DispId(105)> _
    Sub Refresh2(<[In]()> ByRef Level As Object)
    <DispId(503)> _
    Sub ShowBrowserBar(<[In]()> ByRef pvaClsid As Object, <[In]()> ByRef pvarShow As Object, <[In]()> ByRef pvarSize As Object)
    <DispId(106)> _
    Sub [Stop]()

    ' Properties
    Property AddressBar() As <MarshalAs(UnmanagedType.VariantBool)> Boolean
    ReadOnly Property Application() As <MarshalAs(UnmanagedType.IDispatch)> Object
    ReadOnly Property Busy() As <MarshalAs(UnmanagedType.VariantBool)> Boolean
    ReadOnly Property Container() As <MarshalAs(UnmanagedType.IDispatch)> Object
    ReadOnly Property Document() As <MarshalAs(UnmanagedType.IDispatch)> Object
    ReadOnly Property FullName() As <MarshalAs(UnmanagedType.BStr)> String
    Property FullScreen() As <MarshalAs(UnmanagedType.VariantBool)> Boolean
    Property Height() As Integer
    ReadOnly Property HWND() As Integer
    Property Left() As Integer
    ReadOnly Property LocationName() As <MarshalAs(UnmanagedType.BStr)> String
    ReadOnly Property LocationURL() As <MarshalAs(UnmanagedType.BStr)> String
    Property MenuBar() As <MarshalAs(UnmanagedType.VariantBool)> Boolean
    ReadOnly Property Name() As <MarshalAs(UnmanagedType.BStr)> String
    Property Offline() As <MarshalAs(UnmanagedType.VariantBool)> Boolean
    ReadOnly Property Parent() As <MarshalAs(UnmanagedType.IDispatch)> Object
    ReadOnly Property Path() As <MarshalAs(UnmanagedType.BStr)> String
    ReadOnly Property ReadyState() As tagREADYSTATE
    Property RegisterAsBrowser() As <MarshalAs(UnmanagedType.VariantBool)> Boolean
    Property RegisterAsDropTarget() As <MarshalAs(UnmanagedType.VariantBool)> Boolean
    Property Resizable() As <MarshalAs(UnmanagedType.VariantBool)> Boolean
    Property Silent() As <MarshalAs(UnmanagedType.VariantBool)> Boolean
    Property StatusBar() As <MarshalAs(UnmanagedType.VariantBool)> Boolean
    Property StatusText() As <MarshalAs(UnmanagedType.BStr)> String
    Property TheaterMode() As <MarshalAs(UnmanagedType.VariantBool)> Boolean
    Property ToolBar() As Integer
    Property Top() As Integer
    ReadOnly Property TopLevelContainer() As <MarshalAs(UnmanagedType.VariantBool)> Boolean
    ReadOnly Property Type() As <MarshalAs(UnmanagedType.BStr)> String
    Property Visible() As <MarshalAs(UnmanagedType.VariantBool)> Boolean
    Property Width() As Integer
End Interface
