using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class LpProcessor
{
    private List<string> maps = new List<string>();
    private List<string> players = new List<string>();
    private TextWriter sw;
    private string fmtfolder;

    public string Process(string s)
    {
        fmtfolder = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath;
        fmtfolder = Path.Combine(Path.GetDirectoryName(fmtfolder), "LPfmt");

        using (sw = new StringWriter())
        {
            int length;
            List<LpItem> items = LpParser.Parse(s, 0, out length);
            
            var templates = from item in items
                            where item is LpTemplate
                            select item as LpTemplate;
            foreach (LpTemplate template in templates)
            {
                if (template.Name == "MatchList")
                {
                    int matchno = 1;
                    while (true)
                    {
                        List<LpItem> contents;
                        if (!template.Params.TryGetValue("match" + matchno.ToString(), out contents))
                            break;

                        if (contents.Count == 1 && contents[0] is LpTemplate)
                            TryProcessMatchMaps(contents[0] as LpTemplate);
                        matchno++;
                    }
                    sw.WriteLine();
                    continue;
                }
                
                if (TryProcessMatchMaps(template))
                {
                    sw.WriteLine();
                    continue;
                }

                if (TryProcessBracket(template))
                {
                    sw.WriteLine();
                    continue;
                }

                // if we ended up here, we don't support this template
                //sw.WriteLine("; -- Unsupported template: {0} --", template.Name);
            }
            
            string header;
            using (StringWriter headerWriter = new StringWriter())
            {
                if (maps.Count > 0)
                {
                    headerWriter.WriteLine("; maps");
                    int maxlength = (from map in maps select map.Length).Max();
                    foreach (string map in maps)
                        headerWriter.WriteLine("$map {0} = {1}", map.PadRight(maxlength, ' '), map);
                    headerWriter.WriteLine();
                }

                if (players.Count > 0)
                {
                    headerWriter.WriteLine("; players");
                    int maxlength = (from player in players select player.Length).Max();
                    foreach (string player in players)
                        headerWriter.WriteLine("$player {0} = {1}", player.PadRight(maxlength, ' '), player);
                    headerWriter.WriteLine();
                }

                header = headerWriter.ToString();
            }
            return header + sw.ToString();
        }
    }
    private bool TryProcessMatchMaps(LpTemplate template)
    {
        string fmtfile = Path.Combine(fmtfolder, template.Name + ".matchfmt");
        if (!File.Exists(fmtfile))
            return false;

        string[] xs = File.ReadAllLines(fmtfile);
        if (xs.Length != 4)
        {
            sw.WriteLine("; Unrecognised match formatting file.");
            return true;
        }

        sw.WriteLine("; -- {0} --", template.Name);
        string mapname = xs[2];

        int mapno = 1;
        while (true)
        {
            if (TryProcessMatch(template, xs[0], xs[1], xs[2], xs[3], mapno))
                mapno++;
            else
                break;

            if (!mapname.Contains("{0}")) break;
        }
        // just for TeamMatch really
        TryProcessMatch(template, "acep1", "acep2", "acemap", "acewin");

        return true;
    }
    private bool TryProcessBracket(LpTemplate template)
    {
        string fmtfile = Path.Combine(fmtfolder, template.Name + ".bracketfmt");
        if (!File.Exists(fmtfile))
            return false;

        sw.WriteLine("; -- {0} --", template.Name);
        using (var fmtsr = new StreamReader(fmtfile))
        {
            string fmtstring;
            while ((fmtstring = fmtsr.ReadLine()) != null)
            {
                if (string.IsNullOrEmpty(fmtstring) || fmtstring.StartsWith(";"))
                {
                    sw.WriteLine(fmtstring);
                    continue;
                }

                string[] xs = fmtstring.Split(' ');

                if (xs.Length == 2)
                {
                    ProcessBracketGame(template, xs[0], xs[1], "");
                }
                else if (xs.Length == 3)
                {
                    ProcessBracketGame(template, xs[0], xs[1], xs[2]);
                }
                else
                {
                    sw.WriteLine("; Unrecognised format string: " + fmtstring);
                }
            }
            return true;
        }
    }

    private bool TryProcessMatch(LpTemplate template, string p1, string p2, string mapnameparam, string mapwinparam)
    {
        return TryProcessMatch(template, p1, p2, mapnameparam, mapwinparam, null);
    }
    private bool TryProcessMatch(LpTemplate template, string p1, string p2, string mapname, string mapwin, object param)
    {
        string playerleft = template.GetParamText(string.Format(p1, param));
        if (playerleft == null) return false;
        playerleft = playerleft.Replace(" ", "_");
        string playerright = template.GetParamText(string.Format(p2, param));
        if (playerright == null) return false;
        playerright = playerright.Replace(" ", "_");
        
        players.AddUnique(playerleft);
        players.AddUnique(playerright);
        
        string mapnameparam = string.Format(mapname, param);
        string mapwinparam = string.Format(mapwin, param);

        if (!template.Params.ContainsKey(mapwinparam))
            return false;

        string map = "Unknown";
        if (template.Params.ContainsKey(mapnameparam))
        {
            map = template.GetParamText(mapnameparam) ?? "Unknown";
        }
        maps.AddUnique(map);

        string win = template.GetParamText(mapwinparam);
        if (string.IsNullOrEmpty(win) || win == "skip")
            return false;
        string arrow = "?";
        if (win == "1")
            arrow = ">";
        else if (win == "2")
            arrow = "<";

        sw.WriteLine("{0} {2} {1} : {3}", playerleft, playerright, arrow, map);
        return true;
    }
    private void ProcessBracketGame(LpTemplate template, string left, string right, string game)
    {
        if (!template.Params.ContainsKey(left)) return;
        if (!template.Params.ContainsKey(right)) return;

        string playerleft = template.GetParamText(left);
        if (playerleft == null) return;
        playerleft = playerleft.Replace(" ", "_");
        string playerright = template.GetParamText(right);
        if (playerright == null) return;
        playerright = playerright.Replace(" ", "_");

        players.AddUnique(playerleft);
        players.AddUnique(playerright);

        int scoreleft, scoreright;
        if (!int.TryParse(template.GetParamText(left + "score"), out scoreleft)
            | !int.TryParse(template.GetParamText(right + "score"), out scoreright))
        {
            sw.WriteLine(";{0}-{1} {2} {3}", template.GetParamText(left + "score"),
                template.GetParamText(right + "score"), playerleft, playerright);
            return;
        }

        if (template.Params.ContainsKey(game + "details"))
        {
            // game has details - use them
            LpTemplate details = template.GetParamTemplate(game + "details", "BracketMatchSummary");
            for (int i = 1; i <= scoreleft + scoreright; i++)
            {
                string map = "Unknown";
                if (details.Params.ContainsKey("map" + i.ToString()))
                {
                    map = details.GetParamText("map" + i.ToString()) ?? "Unknown";
                }
                maps.AddUnique(map);

                string win = details.GetParamText(string.Format("map{0}win", i));
                if (string.IsNullOrEmpty(win) || win == "skip")
                    continue;
                string arrow = "?";
                if (win == "1")
                    arrow = ">";
                else if (win == "2")
                    arrow = "<";

                sw.WriteLine("{0} {2} {1} : {3}", playerleft, playerright, arrow, map);
            }
        }
        else
        {
            // just output games
            sw.WriteLine("{0}-{1} {2} {3}", scoreleft, scoreright, playerleft, playerright);

            // TODO- extended series
            //|R5W1score2, |R5W2score2=
        }
        
    }
}


public class LpParser
{
    public static List<LpItem> Parse(string s, int start, out int idx)
    {
        return ParseTempl(s, start, out idx, true).Children;
    }
    public static LpComment ParseComment(string s, int start, out int idx)
    {
        idx = start;
        int closingidx = s.IndexOf("-->", idx);
        if (closingidx < 0)
        {
            LpComment comment = new LpComment(s.Substring(idx));
            idx = s.Length;
            return comment;
        }
        else
        {
            LpComment comment = new LpComment(s.Substring(idx, closingidx - idx));
            idx = closingidx + 3;
            return comment;
        }
    }
    public static LpTemplate ParseTempl(string s, int start, out int idx, bool rootlevel)
    {
        idx = start;
        LpTemplate template = new LpTemplate();
        List<LpItem> items = template.Children;
        while (idx < s.Length)
        {
            int commentopenindex = s.IndexOf("<!--", idx);
            int templatecloseindex = rootlevel ? -1 : s.IndexOf("}}", idx);
            int templateopenindex = s.IndexOf("{{", idx);
            if ((commentopenindex >= 0) && ((templatecloseindex < 0) || (commentopenindex < templatecloseindex)) && ((templateopenindex < 0) || (commentopenindex < templateopenindex)))
            {
                string text = s.Substring(idx, commentopenindex - idx).TrimWhitespace();
                if (!string.IsNullOrEmpty(text))
                    items.Add(new LpText(text));
                items.Add(ParseComment(s, commentopenindex + 4, out idx));
            }
            else if ((templateopenindex < 0) || ((templatecloseindex >= 0) && (templatecloseindex < templateopenindex)))
            {
                if (templatecloseindex < 0)
                {
                    string text = s.Substring(idx).TrimWhitespace();
                    if (!string.IsNullOrEmpty(text))
                        items.Add(new LpText(text));
                    idx = s.Length;
                }
                else
                {
                    string text = s.Substring(idx, templatecloseindex - idx).TrimWhitespace();
                    if (!string.IsNullOrEmpty(text))
                        items.Add(new LpText(text));
                    idx = templatecloseindex + 2;
                    break;
                }
            }
            else if (templateopenindex == idx)
            {
                items.Add(ParseTempl(s, templateopenindex + 2, out idx, false));
            }
            else
            {
                string text = s.Substring(idx, templateopenindex - idx);
                if (!string.IsNullOrEmpty(text))
                    items.Add(new LpText(text));
                items.Add(ParseTempl(s, templateopenindex + 2, out idx, false));
            }
        }

        // now process into params
        if (!rootlevel)
        {
            List<LpItem> currentParam = new List<LpItem>();
            int unnamed = 0;
            string paramName = "0";
            Queue<LpItem> remainingItems = new Queue<LpItem>(template.Children);
            while (remainingItems.Count > 0)
            {
                LpItem item = remainingItems.Dequeue();
                if (item is LpText)
                {
                    string text = (item as LpText).Text;
                    int baridx;
                    while ((baridx = text.IndexOf('|')) >= 0)
                    {
                        // add up to the |
                        string tt = text.Substring(0, baridx).TrimWhitespace();
                        if (!string.IsNullOrEmpty(tt))
                            currentParam.Add(new LpText(tt));
                        template.Params[paramName] = currentParam;
                        currentParam = new List<LpItem>();

                        text = text.Substring(baridx + 1);
                        int nextbar = text.IndexOf('|');
                        int equalsidx;
                        if (nextbar > 0)
                            equalsidx = text.IndexOf('=', 0, nextbar);
                        else
                            equalsidx = text.IndexOf('=');
                        if (equalsidx < 0)
                        {
                            unnamed++;
                            paramName = unnamed.ToString();
                        }
                        else
                        {
                            paramName = text.Substring(0, equalsidx).TrimWhitespace();
                            text = text.Substring(equalsidx + 1);
                        }
                    }
                    text = text.TrimWhitespace();
                    if (text.Length > 0) currentParam.Add(new LpText(text));
                }
                else
                {
                    currentParam.Add(item);
                }
            }
            if (currentParam.Count != 0)
            {
                template.Params[paramName] = currentParam;
            }
        }
        return template;
    }
}

public class LpItem { }
public class LpText : LpItem
{
    public LpText(string Text)
    {
        this.Text = Text;
    }
    public string Text;

    public override string ToString()
    {
        return this.Text;
    }
}
public class LpComment : LpItem
{
    public LpComment(string Text)
    {
        this.Text = Text;
    }
    public string Text;

    public override string ToString()
    {
        return this.Text;
    }
}
public class LpTemplate : LpItem
{
    public readonly List<LpItem> Children = new List<LpItem>();
    public readonly Dictionary<string, List<LpItem>> Params = new Dictionary<string, List<LpItem>>();

    public string Name
    {
        get
        {
            if (Params.ContainsKey("0"))
            {
                var p = Params["0"];
                if ((p.Count > 0) && (p[0] is LpText))
                    return (p[0] as LpText).Text;
            }
            return "";
        }
    }
    
    public string GetParamText(string label)
    {
        if (!Params.ContainsKey(label)) return null;
        return (from item in Params[label]
                where item is LpText
                select (item as LpText).Text).FirstOrDefault();
    }
    public LpTemplate GetParamTemplate(string label, string template)
    {
        if (!Params.ContainsKey(label)) return null;
        return (from item in Params[label]
                where item is LpTemplate
                let templ = item as LpTemplate
                where templ.Name == template
                select templ).First();
    }

    public override string ToString()
    {
        return this.Name;
    }
}
