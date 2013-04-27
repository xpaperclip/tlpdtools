TLPD Tools
=========
*** A tool to make adding mass games to TLPD easier ***

**Latest Version:**
v0.2:  http://pit.xpaperclip.net/tlpdtools/v0.2.zip

This tool consists of an LP parser which reads brackets and match lists to generate a game list in a simple text format, and a processor which transforms a game list into BBcode which the 'Add Mass Games' function on TLPD will accept.

Often you can just drag in LP brackets by their [Edit] link, then click Process to generate a game list and then TLPDize the extracted map and player names.

**Note**
This is a very early preview version of this tool, which is functional but still has a lot of quirks. The source code is also architected very poorly and a massive refactoring is definitely on the list of things to do.

The supported bracket formats are also somewhat limited, but are relatively easy to add to (see existing files in the LPfmt folder).


### System Requirements

* Requires [Microsoft .NET Framework 4.0 Client Profile](http://www.microsoft.com/en-au/download/details.aspx?id=17113)
