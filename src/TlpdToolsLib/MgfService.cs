using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.IO;
using FastColoredTextBoxNS;

public class MgfService
{
    public MgfService(FastColoredTextBox tb)
    {
        this.tb = tb;
        tb.AddStyle(preprocStyle);
        tb.AddStyle(commentStyle);
        tb.AddStyle(operatorStyle);
        tb.AddStyle(linkStyle);
        tb.AddStyle(linkTlpdStyle);
        tb.AddStyle(wavyError);
        tb.AddStyle(wavyUndefined);
        tb.AddStyle(wavyRedefined);

        tb.AddStyle(entityStyle);
        tb.AddStyle(seriesStyle);

        //tb.AddStyle(terranStyle);
        //tb.AddStyle(zergStyle);
        //tb.AddStyle(protossStyle);
        //tb.AddStyle(randomStyle);
        tb.AddStyle(playerStyle);
        tb.AddStyle(mapStyle);

        tb.AddStyle(winnerStyle);
        tb.AddStyle(loserStyle);

        tb.AddStyle(rangeStyle);
    }

    public string Filename { get; set; }

    private FastColoredTextBox tb;

    private Style preprocStyle = new TextStyle(Brushes.Red, new SolidBrush(ParseColor("#f5f5e5")), FontStyle.Regular);
    private Style commentStyle = new TextStyle(Brushes.Gray, null, FontStyle.Regular);
    private Style operatorStyle = new TextStyle(Brushes.Red, null, FontStyle.Regular);
    private Style linkStyle = new TextStyle(Brushes.Blue, null, FontStyle.Underline);
    private Style linkTlpdStyle = new TextStyle(Brushes.CornflowerBlue, null, FontStyle.Underline);
    private Style wavyError = new WavyLineStyle(150, Color.Red);
    private Style wavyUndefined = new WavyLineStyle(150, Color.Blue);
    private Style wavyRedefined = new WavyLineStyle(150, Color.Green);

    private Style entityStyle = new TextStyle(Brushes.Maroon, null, FontStyle.Regular);
    private Style seriesStyle = new TextStyle(Brushes.Green, null, FontStyle.Regular);

    //private Style terranStyle = new TextStyle(null, null /* new SolidBrush(ParseColor("#dee7f0")) */, FontStyle.Regular);
    //private Style zergStyle = new TextStyle(null, null /* new SolidBrush(ParseColor("#f0e4de")) */, FontStyle.Regular);
    //private Style protossStyle = new TextStyle(null, null /* new SolidBrush(ParseColor("#dff0de")) */, FontStyle.Regular);
    //private Style randomStyle = new TextStyle(null, null /* new SolidBrush(ParseColor("#eff0de")) */, FontStyle.Regular);
    private Style playerStyle = new TextStyle(null, null /* new SolidBrush(ParseColor("#f5f5f5")) */, FontStyle.Regular);
    private Style mapStyle = new TextStyle(null, null /* new SolidBrush(ParseColor("#f5f5f5")) */, FontStyle.Regular);

    private Style winnerStyle = new TextStyle(null, null /* new SolidBrush(ParseColor("#f5f5f5")) */, FontStyle.Bold);
    private Style loserStyle = new TextStyle(null, null /* new SolidBrush(ParseColor("#f5f5f5")) */, FontStyle.Regular);

    private Style rangeStyle = new TextStyle(Brushes.Green, Brushes.LightGray, FontStyle.Regular);

    private readonly Dictionary<string, TlpdEntity> players = new Dictionary<string, TlpdEntity>();
    private readonly Dictionary<string, TlpdEntity> maps = new Dictionary<string, TlpdEntity>();
    private readonly Dictionary<string, int> playerRaceImageIndex = new Dictionary<string, int>() { { "T", 0 }, { "Z", 1 }, { "P", 2 }, { "R", 3 } };

    private readonly List<string> seriesOrder = new List<string>();

    public void Colourize()
    {
        try
        {
            Colourize(false);
        }
        catch
        {
            // try again
            tb.ClearStylesBuffer();
            Colourize(false);
        }
    }
    public void Colourize(bool resolveReferences)
    {
        if (resolveReferences)
            tb.ClearStyle(FastColoredTextBoxNS.StyleIndex.All);
        else
            tb.ClearStyle(tb.GetStyleIndexMask(new Style[] { preprocStyle, linkStyle, commentStyle }));

        players.Clear();
        maps.Clear();
        seriesOrder.Clear();

        var p = new Processing(this);
        using (var sr = new StringReader(tb.Text))
        {
            p.Do(sr, true, resolveReferences, null);
        }
    }
    public string Process()
    {
        players.Clear();
        maps.Clear();
        seriesOrder.Clear();

        var p = new Processing(this);
        using (var sr = new StringReader(tb.Text))
        using (var sw = new StringWriter())
        {
            p.Do(sr, false, true, sw);
            return sw.ToString();
        }
    }

    public void AutocompleteNeeded(AutocompleteMenu autocomplete, AutocompleteNeededEventArgs e)
    {
        if (autocomplete.Visible)
            return;
        
        int linenum = tb.Selection.Start.iLine;
        string line = tb.GetLine(linenum).Text;
        int linestart, lineend;
        line = line.Trim(out linestart, out lineend);
        
        Range range = new Range(tb);
        range.Start = new Place(linestart, linenum);
        range.End = new Place(lineend, linenum);

        //if (line.StartsWith("$series "))
        //{
        //    int startidx = "$series ".Length;
        //    range.Start = new Place(linestart + startidx, linenum);
        //    Range maprange = new Range(tb);
        //    while (startidx < line.Length)
        //    {
        //        int idx = line.IndexOf(",", startidx);
        //        if (idx < 0) idx = line.Length;

        //        string map = line.Substring(startidx, idx - startidx);
        //        int mapstart;
        //        map = map.TrimLeft(out mapstart);

        //        maprange.Start = new Place(linestart + startidx + mapstart, linenum);
        //        maprange.End = new Place(linestart + startidx + map.Length, linenum);
        //        if (maprange.Contains(tb.Selection.Start))
        //        {
        //            range = maprange;
        //            break;
        //        }

        //        startidx = idx + 1;
        //    }
        //    if (startidx == line.Length)
        //    {
        //        maprange.Start = new Place(linestart + startidx, linenum);
        //        maprange.End = new Place(linestart + startidx, linenum);
        //    }

        //    e.Handled = true;
        //    e.Fragment = range;
        //    tb.ClearStyle(tb.GetStyleIndexMask(new Style[] { rangeStyle }));
        //    range.SetStyle(rangeStyle);
        //    autocomplete.Items.SetAutocompleteItems((from key in maps.Keys
        //                                             orderby key
        //                                             select new AutocompleteItem(key)).ToList());
        //    return;
        //}

        if (e.Forced && line.StartsWith("$series"))
        {
            autocomplete.Items.SetAutocompleteItems((from key in maps.Keys
                                                     orderby key
                                                     select new AutocompleteItem(key)).ToList());
            return;
        }

        if (e.Forced)
        {
            // default to combined list
            autocomplete.Items.SetAutocompleteItems((from kvp in players
                                                     orderby kvp.Key
                                                     select new AutocompleteItem(kvp.Key, playerRaceImageIndex.GetValueOrDefault(kvp.Value.Race, -1))
                                                     ).Concat(
                                                     from key in maps.Keys
                                                     orderby key
                                                     select new AutocompleteItem(key)).ToList());
            return;
        }

        autocomplete.Items.SetAutocompleteItems(new List<AutocompleteItem>());
    }

    private class Processing
    {
        public Processing(MgfService service)
        {
            this.svc = service;
        }
        public string processingFilename = null;

        private MgfService svc;
        private FastColoredTextBox tb;
        bool doColour;

        string line = null;
        int linenum = -1;
        int lineend = -1;
        int linestart = -1;
        

        private void Colour(int start, int end, Style style)
        {
            if (doColour) tb.GetRange(new Place(linestart + start, linenum), new Place(linestart + end + 1, linenum)).SetStyle(style);
        }
        private void ColourLength(int start, int length, Style style)
        {
            if (doColour) Colour(start, start + length - 1, style);
        }
        private void ColourToEnd(int start, Style style)
        {
            if (doColour) Colour(start, lineend, style);
        }

        public void Do(TextReader sr, bool doColour, bool resolveReferences, TextWriter writer)
        {
            if (doColour)
            {
                tb = svc.tb;
                this.doColour = doColour;
            }
            bool generateOutput = false;
            if (writer != null)
            {
                // if we are generating output, then force references to be resolved
                resolveReferences = true;
                generateOutput = true;
            }

            while ((line = sr.ReadLine()) != null)
            {
                linenum++;

                // syntax colour comments
                int commentidx = line.IndexOf(';');
                if (commentidx >= 0)
                {
                    if (doColour) tb.GetRange(new Place(commentidx, linenum), new Place(line.Length, linenum)).SetStyle(svc.commentStyle);
                    line = line.Substring(0, commentidx);
                }

                // trim line
                TrimLine(true);

                // if nothing left, skip line
                if (line.Length == 0) continue;

                // termination
                if (IsCommand("$end", svc.preprocStyle))
                {
                    // terminate further processing and colour as a comment
                    if (doColour) tb.GetRange(new Place(linestart, linenum), tb.Range.End).SetStyle(svc.commentStyle);
                    break;
                }

                // include file
                if (IsCommand("$include", svc.preprocStyle))
                {
                    ColourToEnd(0, svc.linkStyle);
                    if (resolveReferences)
                    {
                        string filename = svc.ResolveIncludeFile(line);

                        if (filename == null)
                        {
                            // doesn't exist
                            ColourToEnd(0, svc.wavyError);
                        }
                        else
                        {
                            // open included file and process it
                            var psub = new Processing(svc);
                            psub.processingFilename = filename;
                            using (var subreader = new StreamReader(filename))
                            {
                                psub.Do(subreader, false, true, writer);
                            }
                        }
                    }
                    continue;
                }

                // print line
                if (IsCommand("$print", svc.preprocStyle))
                {
                    if (generateOutput)
                        writer.WriteLine(line);
                    continue;
                }

                // tlpd line
                if (line.StartsWith("[tlpd#"))
                {
                    int endidx;
                    TlpdEntity entity = Tlpd.Parse(line, false, out endidx);
                    if (endidx > 0)
                    {
                        ColourLength(0, endidx, svc.linkTlpdStyle);
                        if (!entity.IsValid || string.IsNullOrWhiteSpace(entity.Name))
                            ColourLength(0, endidx, svc.wavyError);
                        switch (entity.Type)
                        {
                            case TlpdEntityType.Map: svc.maps[entity.Name] = entity; break;
                            case TlpdEntityType.Player: svc.players[entity.Name] = entity; break;
                        }
                    }
                    if (endidx < line.Length)
                        Colour(endidx, line.Length, svc.wavyError);
                    continue;
                }

                // map line
                if (IsCommand("$map", svc.entityStyle))
                {
                    if (!line.Contains("="))
                    {
                        ErrorLine("Syntax error: Try '$map name = [tlpd#maps#000#db]' instead.");
                        continue;
                    }

                    // $map Name = [tlpd#]
                    int equalsidx = line.IndexOf("=");
                    string name = line.Substring(0, equalsidx);
                    string tlpd = line.Substring(equalsidx + 1);

                    int namestart, nameend;
                    name = name.Trim(out namestart, out nameend);
                    if (resolveReferences)
                    {
                        if (svc.maps.ContainsKey(name))
                            Colour(namestart, nameend, svc.wavyRedefined);
                    }

                    int tlpdstart, tlpdend;
                    tlpd = tlpd.Trim(out tlpdstart, out tlpdend);
                    int endidx;
                    TlpdEntity entity = Tlpd.Parse(tlpd, true, out endidx);
                    if (endidx > 0)
                    {
                        ColourLength(equalsidx + 1 + tlpdstart, endidx, svc.linkTlpdStyle);
                        if (entity.Type == TlpdEntityType.Map)
                        {
                            entity.Name = name;
                            svc.maps[entity.Name] = entity;
                        }
                        else
                        {
                            ColourLength(equalsidx + 1 + tlpdstart, endidx, svc.wavyError);
                        }
                    }
                    if (endidx < line.Length) Colour(equalsidx + 1 + tlpdstart + endidx, line.Length - 1, svc.wavyError);

                    //if (resolveReferences)
                    //{
                    //    svc.maps[name] = Tlpd.Parse(tlpd);
                    //    Colour(namestart, nameend, svc.mapStyle);
                    //}
                    continue;
                }

                // player line
                if (IsCommand("$player", svc.entityStyle))
                {
                    if (!line.Contains("="))
                    {
                        ErrorLine("Syntax error: Try '$player name = [tlpd#players#0#T#db]' instead.");
                        continue;
                    }

                    // $map Name = [tlpd#]
                    int equalsidx = line.IndexOf("=");
                    string name = line.Substring(0, equalsidx);
                    string tlpd = line.Substring(equalsidx + 1);

                    int namestart, nameend;
                    name = name.Trim(out namestart, out nameend);
                    if (resolveReferences)
                    {
                        if (svc.players.ContainsKey(name))
                            Colour(namestart, nameend, svc.wavyRedefined);
                    }

                    int tlpdstart, tlpdend;
                    tlpd = tlpd.Trim(out tlpdstart, out tlpdend);
                    int endidx;
                    TlpdEntity entity = Tlpd.Parse(tlpd, true, out endidx);
                    if (endidx > 0)
                    {
                        ColourLength(equalsidx + 1 + tlpdstart, endidx, svc.linkTlpdStyle);
                        if (entity.Type == TlpdEntityType.Player)
                        {
                            entity.Name = name;
                            svc.players[entity.Name] = entity;
                        }
                        else
                        {
                            ColourLength(equalsidx + 1 + tlpdstart, endidx, svc.wavyError);
                        }
                    }
                    if (endidx < line.Length) Colour(equalsidx + 1 + tlpdstart + endidx, line.Length - 1, svc.wavyError);

                    //if (resolveReferences)
                    //{
                    //    svc.players[name] = Tlpd.Parse(tlpd);
                    //    Colour(namestart, nameend, ColourPlayer(svc.players[name]));
                    //}
                    continue;
                }

                // series line
                if (IsCommand("$series", svc.seriesStyle))
                {
                    svc.seriesOrder.Clear();
                    int startidx = 0;
                    while (startidx < line.Length)
                    {
                        int idx = line.IndexOf(",", startidx);
                        if (idx < 0) idx = line.Length;

                        string map = line.Substring(startidx, idx - startidx);
                        int mapstart, mapend;
                        map = map.Trim(out mapstart, out mapend);
                        svc.seriesOrder.Add(map);
                        if (resolveReferences)
                        {
                            if (!svc.maps.ContainsKey(map))
                                Colour(startidx + mapstart, startidx + mapend, svc.wavyUndefined);
                            else
                                Colour(startidx + mapstart, startidx + mapend, svc.mapStyle);
                        }

                        startidx = idx + 1;
                    }

                    continue;
                }

                // winner > loser : map
                // winner < loser : map
                if (line.Contains(":"))
                {
                    int colonidx = line.IndexOf(":");
                    string players = line.Substring(0, colonidx);
                    string map = line.Substring(colonidx + 1);

                    Colour(colonidx, colonidx, svc.operatorStyle);

                    int mapstart, mapend;
                    map = map.Trim(out mapstart, out mapend);
                    if (resolveReferences)
                    {
                        if (!svc.maps.ContainsKey(map))
                            Colour(colonidx + 1 + mapstart, colonidx + 1 + mapend, svc.wavyUndefined);
                        else
                            Colour(colonidx + 1 + mapstart, colonidx + 1 + mapend, svc.mapStyle);
                    }

                    int playersstart, playersend;
                    players = players.Trim(out playersstart, out playersend);

                    int arrowidx = 0;
                    bool leftwinner;
                    if (players.Contains(">"))
                    {
                        arrowidx = players.IndexOf(">");
                        leftwinner = true;
                    }
                    else if (players.Contains("<"))
                    {
                        arrowidx = players.IndexOf("<");
                        leftwinner = false;
                    }
                    else
                    {
                        Error(playersstart, playersend, "No winner specified.");
                        continue;
                    }

                    string left = players.Substring(0, arrowidx);
                    string right = players.Substring(arrowidx + 1);
                    Colour(arrowidx, arrowidx, svc.operatorStyle);

                    int leftstart, leftend;
                    left = left.Trim(out leftstart, out leftend);
                    if (resolveReferences)
                    {
                        if (!svc.players.ContainsKey(left))
                            Colour(leftstart, leftend, svc.wavyUndefined);
                        else
                            Colour(leftstart, leftend, ColourPlayer(svc.players[left]));
                    }

                    int rightstart, rightend;
                    right = right.Trim(out rightstart, out rightend);
                    if (resolveReferences)
                    {
                        if (!svc.players.ContainsKey(right))
                            Colour(arrowidx + 1 + rightstart, arrowidx + 1 + rightend, svc.wavyUndefined);
                        else
                            Colour(arrowidx + 1 + rightstart, arrowidx + 1 + rightend, ColourPlayer(svc.players[right]));
                    }

                    if (generateOutput)
                        WriteGame(writer, left, right, map, leftwinner);

                    continue;
                }

                // map @ winner loser
                if (line.Contains("@"))
                {
                    int atidx = line.IndexOf("@");
                    string map = line.Substring(0, atidx);
                    string players = line.Substring(atidx + 1);

                    Colour(atidx, atidx, svc.operatorStyle);

                    int mapstart, mapend;
                    map = map.Trim(out mapstart, out mapend);
                    if (resolveReferences)
                    {
                        if (!svc.maps.ContainsKey(map))
                            Colour(mapstart, mapend, svc.wavyUndefined);
                        else
                            Colour(mapstart, mapend, svc.mapStyle);
                    }

                    int playersstart, playersend;
                    players = players.Trim(out playersstart, out playersend);
                    line = line.Substring(atidx + 1 + playersstart);
                    linestart += atidx + 1 + playersstart;

                    int spaceidx = players.IndexOf(" ");
                    if (spaceidx < 0)
                    {
                        Error(playersstart, playersend, "No winner specified.");
                        continue;
                    }

                    string left = players.Substring(0, spaceidx);
                    string right = players.Substring(spaceidx + 1);

                    int leftstart, leftend;
                    left = left.Trim(out leftstart, out leftend);
                    if (resolveReferences)
                    {
                        if (!svc.players.ContainsKey(left))
                            Colour(leftstart, leftend, svc.wavyUndefined);
                        else
                            Colour(leftstart, leftend, ColourPlayer(svc.players[left]));
                    }

                    int rightstart, rightend;
                    right = right.Trim(out rightstart, out rightend);
                    if (resolveReferences)
                    {
                        if (!svc.players.ContainsKey(right))
                            Colour(spaceidx + 1 + rightstart, spaceidx + 1 + rightend, svc.wavyUndefined);
                        else
                            Colour(spaceidx + 1 + rightstart, spaceidx + 1 + rightend, ColourPlayer(svc.players[right]));
                    }

                    if (generateOutput)
                        WriteGame(writer, left, right, map);

                    continue;
                }

                // 2-0 left right
                if (line.Contains("-"))
                {
                    int atidx = line.IndexOf(" ");
                    if (atidx < 0)
                    {
                        ErrorLine("Syntax error");
                        continue;
                    }

                    string score = line.Substring(0, atidx);
                    string players = line.Substring(atidx + 1);

                    Colour(atidx, atidx, svc.operatorStyle);

                    int scorestart, scoreend;
                    score = score.Trim(out scorestart, out scoreend);

                    // parse scores
                    int dashidx = score.IndexOf("-");
                    if (dashidx < 0)
                    {
                        ErrorLine("Syntax error");
                        continue;
                    }

                    int scoreleft, scoreright;
                    bool errorfree = true;
                    if (!int.TryParse(score.Substring(0, dashidx), out scoreleft))
                    {
                        Error(scorestart, scorestart + dashidx - 1, "Error");
                        errorfree = false;
                    }
                    if (!int.TryParse(score.Substring(dashidx + 1), out scoreright))
                    {
                        Error(scorestart + dashidx + 1, scoreend, "Error");
                        errorfree = false;
                    }
                    Colour(scorestart + dashidx, scorestart + dashidx, svc.operatorStyle);
                    if (errorfree)
                    {
                        Colour(scorestart, scorestart + dashidx - 1, (scoreleft > scoreright) ? svc.winnerStyle : svc.loserStyle);
                        Colour(scorestart + dashidx + 1, scoreend, (scoreleft < scoreright) ? svc.winnerStyle : svc.loserStyle);
                    }

                    // parse players
                    int playersstart, playersend;
                    players = players.Trim(out playersstart, out playersend);
                    line = line.Substring(atidx + 1 + playersstart);
                    linestart += atidx + 1 + playersstart;

                    int spaceidx = players.IndexOf(" ");
                    if (spaceidx < 0)
                    {
                        Error(playersstart, playersend, "No winner specified.");
                        continue;
                    }

                    string left = players.Substring(0, spaceidx);
                    string right = players.Substring(spaceidx + 1);

                    int leftstart, leftend;
                    left = left.Trim(out leftstart, out leftend);
                    if (resolveReferences)
                    {
                        if (!svc.players.ContainsKey(left))
                            Colour(leftstart, leftend, svc.wavyUndefined);
                        else
                            Colour(leftstart, leftend, ColourPlayer(svc.players[left]));
                    }

                    int rightstart, rightend;
                    right = right.Trim(out rightstart, out rightend);
                    if (resolveReferences)
                    {
                        if (!svc.players.ContainsKey(right))
                            Colour(spaceidx + 1 + rightstart, spaceidx + 1 + rightend, svc.wavyUndefined);
                        else
                            Colour(spaceidx + 1 + rightstart, spaceidx + 1 + rightend, ColourPlayer(svc.players[right]));
                    }

                    if (generateOutput)
                    {
                        bool alternate = (scoreleft > scoreright);
                        int mapsdone = 0;
                        while (scoreleft + scoreright > 0)
                        {
                            bool leftwinner = true;
                            if (alternate)
                                leftwinner = (scoreleft > 0);
                            else
                                leftwinner = !(scoreright > 0);
                            alternate = !alternate;
                            if (leftwinner)
                                scoreleft--;
                            else
                                scoreright--;

                            string map = "";
                            if (mapsdone < svc.seriesOrder.Count) map = svc.seriesOrder[mapsdone];
                            mapsdone++;
                            
                            WriteGame(writer, left, right, map, leftwinner);
                        }
                    }

                    continue;
                }

                // wlw winner loser
                int satidx = line.IndexOf(" ");
                if (satidx >= 0)
                {
                    string score = line.Substring(0, satidx);
                    string players = line.Substring(satidx + 1);

                    int scorestart, scoreend;
                    score = score.Trim(out scorestart, out scoreend);

                    // parse scores
                    bool[] leftwinner = new bool[score.Length];
                    bool errorfree = true;
                    for (int i = 0; i < score.Length; i++)
                    {
                        char c = score[i];
                        if (c == 'w' || c == 'W')
                        {
                            leftwinner[i] = true;
                        }
                        else if (c == 'l' || c == 'L')
                        {
                            leftwinner[i] = false;
                        }
                        else
                        {
                            errorfree = false;
                            break;
                        }
                    }
                    if (!errorfree)
                    {
                        ErrorLine("Syntax error");
                        continue;
                    }
                    for (int i = 0; i < score.Length; i++)
                    {
                        ColourLength(scorestart + i, 1, leftwinner[i] ? svc.winnerStyle : svc.loserStyle);
                    }

                    // parse players
                    int playersstart, playersend;
                    players = players.Trim(out playersstart, out playersend);
                    line = line.Substring(satidx + 1 + playersstart);
                    linestart += satidx + 1 + playersstart;

                    int spaceidx = players.IndexOf(" ");
                    if (spaceidx < 0)
                    {
                        Error(playersstart, playersend, "No winner specified.");
                        continue;
                    }

                    string left = players.Substring(0, spaceidx);
                    string right = players.Substring(spaceidx + 1);

                    int leftstart, leftend;
                    left = left.Trim(out leftstart, out leftend);
                    if (resolveReferences)
                    {
                        if (!svc.players.ContainsKey(left))
                            Colour(leftstart, leftend, svc.wavyUndefined);
                        else
                            Colour(leftstart, leftend, ColourPlayer(svc.players[left]));
                    }

                    int rightstart, rightend;
                    right = right.Trim(out rightstart, out rightend);
                    if (resolveReferences)
                    {
                        if (!svc.players.ContainsKey(right))
                            Colour(spaceidx + 1 + rightstart, spaceidx + 1 + rightend, svc.wavyUndefined);
                        else
                            Colour(spaceidx + 1 + rightstart, spaceidx + 1 + rightend, ColourPlayer(svc.players[right]));
                    }

                    if (generateOutput)
                    {
                        int mapsdone = 0;
                        for (int i = 0; i < leftwinner.Length; i++)
                        {
                            string map = "";
                            if (mapsdone < svc.seriesOrder.Count) map = svc.seriesOrder[mapsdone];
                            mapsdone++;

                            WriteGame(writer, left, right, map, leftwinner[i]);
                        }
                    }

                    continue;
                }

                // unrecognised line
                ErrorLine("unrecognised command");
            }
        }

        private void TrimLine()
        {
            TrimLine(false);
        }
        private void TrimLine(bool reset)
        {
            if (reset)
            {
                lineend = line.Length - 1;
                linestart = 0;
            }
            int end = line.Length - 1;
            int start = 0;
            int reduction = 0;
            while (start < line.Length)
            {
                if (!char.IsWhiteSpace(line[start]))
                    break;
                start++;
            }
            while (end >= start)
            {
                if (!char.IsWhiteSpace(line[end]))
                    break;
                end--;
                reduction++;
            }
            line = line.Substring(start, end - start + 1);
            
            linestart += start;
            lineend -= reduction;
        }

        private bool IsCommand(string command, Style style)
        {
            bool iscmd = (line == command || line.StartsWith(command + " "));
            if (!iscmd) return false;

            ColourLength(0, command.Length, style);
            
            // move line forward
            line = line.Substring(command.Length);
            linestart += command.Length;
            TrimLine();
            
            return true;
        }
        
        private void Error(int start, int end, string error)
        {
            Colour(start, end, svc.wavyError);
        }
        private void ErrorLine(string error)
        {
            ColourToEnd(0, svc.wavyError);
        }

        private Style ColourPlayer(TlpdEntity player)
        {
            return svc.playerStyle;

            //switch (player.Race)
            //{
            //    case "T": return svc.terranStyle;
            //    case "Z": return svc.zergStyle;
            //    case "P": return svc.protossStyle;
            //    case "R": return svc.randomStyle;
            //}
            return null;
        }
        private void WriteGame(TextWriter writer, string winner, string loser, string map)
        {
            WriteGame(writer, winner, loser, map, true);
        }
        private void WriteGame(TextWriter writer, string left, string right, string map, bool leftwinner)
        {
            string winnercode = leftwinner ? GetCodeOrDefault(svc.players, left) : GetCodeOrDefault(svc.players, right);
            string losercode = leftwinner ? GetCodeOrDefault(svc.players, right) : GetCodeOrDefault(svc.players, left);
            string mapcode = GetCodeOrDefault(svc.maps, map);
            if (mapcode.Length == 0) mapcode = "[tlpd#maps#0#a]Unknown[/tlpd]";
            writer.WriteLine("[b]{0}[/b] < {2} > {1}", winnercode, losercode, mapcode);
        }
        private string GetCodeOrDefault(Dictionary<string, TlpdEntity> dict, string key)
        {
            TlpdEntity value;
            if (dict.TryGetValue(key, out value)) return value.CodeSimple;
            return key;
        }
    }

    public bool CharIsHyperlink(Place place)
    {
        var linkStyleIndex = tb.GetStyleIndexMask(new Style[] { linkStyle, linkTlpdStyle });
        if (place.iChar < tb.GetLineLength(place.iLine))
            if ((tb[place].style & linkStyleIndex) != 0)
                return true;

        return false;
    }
    public string GetLinkUrl(Place p, TlpdDatabase tlpd)
    {
        var linkStyleIndex = tb.GetStyleIndexMask(new Style[] { linkStyle });
        var tlpdStyleIndex = tb.GetStyleIndexMask(new Style[] { linkTlpdStyle });
        string text = tb.GetRange(p, p).GetFragment(@"[\S]").Text;

        if ((tb[p].style & linkStyleIndex) != 0)
        {
            return "$" + ResolveIncludeFile(text);
        }

        if ((tb[p].style & tlpdStyleIndex) != 0)
        {
            TlpdEntity entity = Tlpd.Parse(text, true);
            if (string.IsNullOrEmpty(entity.Database)) entity.Database = tlpd.Url;
            return entity.Url;
        }

        return text;
    }

    public string ResolveIncludeFile(string filenamecore)
    {
        string includefolder = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath;
        includefolder = Path.Combine(Path.GetDirectoryName(includefolder), "include");

        string filename = null;
        if (this.Filename != null)
        {
            // see if it exists in the same folder
            filename = Path.Combine(Path.GetDirectoryName(this.Filename), filenamecore);
            if (!File.Exists(filename))
                filename = null;
        }
        if (filename == null)
        {
            // otherwise, check the include folder
            filename = Path.Combine(includefolder, filenamecore);
            if (!File.Exists(filename))
                filename = null;
        }
        if (filename == null)
        {
            // last scenario, just open whatever and hope
            filename = filenamecore;
            if (!File.Exists(filename))
                filename = null;
        }

        return filename;
    }

    private static Color ParseColor(string s)
    {
        if (s.StartsWith("#"))
        {
            if (s.Length <= 7)
                return Color.FromArgb(255, Color.FromArgb(Int32.Parse(s.Substring(1), System.Globalization.NumberStyles.AllowHexSpecifier)));
            else
                return Color.FromArgb(Int32.Parse(s.Substring(1), System.Globalization.NumberStyles.AllowHexSpecifier));
        }
        else
            return Color.FromName(s);
    }
}
