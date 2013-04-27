using System;
using System.IO;
using System.Net;

public static class Tlpd
{
    public static HttpWebRequest GetRequest(string request, TlpdDatabase db)
    {
        var encoded = System.Uri.EscapeDataString(request);
        var requestString = string.Format("text={0}&mode=tlpd-{1}", encoded, db.TlpdizeMode);

        System.Net.ServicePointManager.Expect100Continue = false;
        var webRequest = (HttpWebRequest)WebRequest.Create("http://www.teamliquid.net/tlpd/tlpdize.php");
        webRequest.Method = "POST";
        webRequest.ContentType = "application/x-www-form-urlencoded";
        webRequest.ContentLength = requestString.Length;
        webRequest.Accept = "*/*";
        webRequest.Timeout = 10000;
        using (var stream = webRequest.GetRequestStream())
        using (var sw = new StreamWriter(stream))
        {
            sw.Write(requestString);
        }

        return webRequest;
    }
    public static string Request(string request, TlpdDatabase db)
    {
        var webRequest = GetRequest(request, db);
        var response = webRequest.GetResponse();
        using (var stream = response.GetResponseStream())
        using (var sr = new StreamReader(stream))
        {
            return sr.ReadToEnd();
        }
    }

    public static TlpdEntity Parse(string tlpd)
    {
        return Parse(tlpd, true);
    }
    public static TlpdEntity Parse(string tlpd, bool allowJustOpen)
    {
        int length;
        return Parse(tlpd, allowJustOpen, out length);
    }
    public static TlpdEntity Parse(string tlpd, bool allowJustOpen, out int length)
    {
        if (!tlpd.StartsWith("[tlpd#"))
        {
            // reject it immediately
            length = 0;
            return TlpdEntity.InvalidEntity;
        }

        // find end of tlpd string
        int closebracket = tlpd.IndexOf("]");
        if (closebracket < 0)
        {
            // incomplete tag, reject
            length = 0;
            return TlpdEntity.InvalidEntity;
        }

        // find closing tag if required
        TlpdEntity entity = new TlpdEntity();
        int closingtag = tlpd.IndexOf("[/tlpd]");
        if (closingtag < 0)
        {
            length = closebracket + 1;
            if (!allowJustOpen)
            {
                // incomplete tag, reject it now
                return TlpdEntity.InvalidEntity;
            }
        }
        else
        {
            length = closingtag + "[/tlpd]".Length;
            if (closebracket > closingtag)
            {
                length = closebracket;
                return TlpdEntity.InvalidEntity;
            }
            entity.Name = tlpd.Substring(closebracket + 1, closingtag - (closebracket + 1));
        }

        // get tag bits
        string param = tlpd.Substring(0, closebracket);
        param = param.Substring("[tlpd#".Length);
        string[] tags = param.Split('#');
        if (tags.Length < 2)
        {
            // not enough arguments, reject it
            return TlpdEntity.InvalidEntity;
        }

        entity.Id = tags[1];
        switch (tags[0])
        {
            case "maps":
                entity.Type = TlpdEntityType.Map;
                if (tags.Length > 2) entity.Database = tags[2];
                if (tags.Length > 3) entity.Race = tags[3];
                break;
            case "players":
                // players must have a race
                if (tags.Length < 3) return TlpdEntity.InvalidEntity;
                entity.Type = TlpdEntityType.Player;
                entity.Race = tags[2];
                if (tags.Length > 3) entity.Database = tags[3];
                break;
            default:
                return TlpdEntity.InvalidEntity;
        }

        // [tlpd#maps#id#db#tileset]name[/tlpd]
        // [tlpd#players#id#R#db]name[/tlpd]

        return entity;
    }
}

public struct TlpdDatabase
{
    public TlpdDatabase(string name, string url, string mode)
    {
        this.Name = name;
        this.Url = url;
        this.TlpdizeMode = mode;
    }

    public string Name;
    public string Url;
    public string TlpdizeMode;

    public override string ToString()
    {
        return this.Name;
    }
}

public enum TlpdEntityType
{
    Invalid, Player, Map
}
public struct TlpdEntity
{
    public static TlpdEntity InvalidEntity
    {
        get { return new TlpdEntity() { Type = TlpdEntityType.Invalid }; }
    }

    public TlpdEntityType Type;
    public string Id;
    public string Race;
    public string Database;
    public string Name;

    public bool IsValid
    {
        get { return (this.Type == TlpdEntityType.Map) || (this.Type == TlpdEntityType.Player); }
    }
    public string Url
    {
        get
        {
            if (this.Type == TlpdEntityType.Invalid)
                return "http://www.teamliquid.net/tlpd/";
            else
                return string.Format("http://www.teamliquid.net/tlpd/{0}/{1}/{2}",
                    this.Database, this.Type.ToString().ToLower() + "s", this.Id);
        }
    }
    public string Code
    {
        get
        {
            switch (Type)
            {
                case TlpdEntityType.Map:
                    return string.Format("[tlpd#maps#{0}#{2}{1}]{3}[/tlpd]", Id, (Race.Length > 0 ? "#" + Race : ""), Database, Name);
                case TlpdEntityType.Player:
                    return string.Format("[tlpd#players#{0}#{1}#{2}]{3}[/tlpd]", Id, Race, Database, Name);
                default:
                    return string.Empty;
            }
        }
    }
    public string CodeSimple
    {
        get
        {
            switch (Type)
            {
                case TlpdEntityType.Map:
                    return string.Format("[tlpd#maps#{0}#{2}]{3}[/tlpd]", Id, Race, (!string.IsNullOrWhiteSpace(Database) ? Database : "a"), Name);
                case TlpdEntityType.Player:
                    return string.Format("[tlpd#players#{0}#{1}]{3}[/tlpd]", Id, Race, Database, Name);
                default:
                    return string.Empty;
            }
        }
    }
    public override string ToString()
    {
        return this.Code;
    }
}
