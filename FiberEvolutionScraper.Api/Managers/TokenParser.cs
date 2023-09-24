namespace FiberEvolutionScraper.Api.Services;

public class TokenParser
{
    internal string Token;
    internal DateTime TokenAge = DateTime.Now;

    /// <summary>
    /// e.map(n=>(n >= "A" && n <= "Z" ? 
    /// n = String.fromCharCode("Z".charCodeAt(0) - (n.charCodeAt(0) - "A".charCodeAt(0))).toLowerCase() : 
    /// n >= "a" && n <= "z" ? 
    /// n = String.fromCharCode("z".charCodeAt(0) - (n.charCodeAt(0) - "a".charCodeAt(0))).toUpperCase() : 
    /// n >= "0" && n <= "9" && (n = String.fromCharCode("9".charCodeAt(0) - (n.charCodeAt(0) - "0".charCodeAt(0)))), n))
    /// 
    /// for (let i = 0; i < n; i++)
    ///     r.push(e[i * t % n]);
    /// e.reverse();
    /// 
    /// e.split(61) et inverser les 2 parties
    /// </summary>

    public string GetToken(string token)
    {
        int n = 115;
        int t = 13;
        int stringSplitIndex = 61;

        var step1 = "";
        token.ToCharArray().ToList().ForEach(s =>
        {
            if (s >= 'A' && s <= 'Z')
            {
                step1 += Convert.ToChar(Convert.ToChar('Z' - (s - 'A')).ToString().ToLower());
            }
            else if (s >= 'a' && s <= 'z')
            {
                step1 += Convert.ToChar(Convert.ToChar('z' - (s - 'a')).ToString().ToUpper());
            }
            else if (s >= '0' && s <= '9')
            {
                step1 += Convert.ToChar('9' - (s - '0'));
            }
            else
            {
                step1 += s;
            }
        });

        var step2 = "";
        for (int i = 0; i < n; i++)
        {
            step2 += step1[i * t % n];
        }

        step2 = string.Concat(step2.ToCharArray().Reverse());

        Token = step2.Substring(stringSplitIndex, step2.Length-stringSplitIndex) + step2.ToString().Substring(0, stringSplitIndex);
        TokenAge = DateTime.Now;
        Console.WriteLine("New Token set !");

        return Token;
    }

    ///  function aB(e, n, t) {
    ///  var r = (e = e || {}).random || (e.rng || eB)();
    ///  if (r[6] = 15 & r[6] | 64,
    ///  r[8] = 63 & r[8] | 128,
    ///  n) {
    ///     t = t || 0;
    ///     for (var i = 0; i< 16; ++i)
    ///         n[t + i] = r[i];
    ///         return n
    ///  }
    ///  return function iB(e) {
    ///  t = (Sn[e[n + 0]] + Sn[e[n + 1]] + Sn[e[n + 2]] + Sn[e[n + 3]] + "-" + Sn[e[n + 4]] + Sn[e[n + 5]] + "-" + Sn[e[n + 6]] + Sn[e[n + 7]] + "-" + Sn[e[n + 8]] + Sn[e[n + 9]] + "-" + Sn[e[n + 10]] + Sn[e[n + 11]] + Sn[e[n + 12]] + Sn[e[n + 13]] + Sn[e[n + 14]] + Sn[e[n + 15]]).toLowerCase();
    ///  return t
    ///  }
    ///  (r)
    ///  
    ///  
    /// Sn = [00, 01, 02, fe, ff]
    /// (Sn[e[n + 0]] + Sn[e[n + 1]] + Sn[e[n + 2]] + Sn[e[n + 3]] + "-" + Sn[e[n + 4]] + Sn[e[n + 5]] + "-" + Sn[e[n + 6]] + Sn[e[n + 7]] + "-" + Sn[e[n + 8]] + Sn[e[n + 9]] + "-" + Sn[e[n + 10]] + Sn[e[n + 11]] + Sn[e[n + 12]] + Sn[e[n + 13]] + Sn[e[n + 14]] + Sn[e[n + 15]]).toLowerCase()
    /// e = uint8array

    public string GenerateAppId()
    {
        var uint8array = Guid.NewGuid().ToByteArray();

        var result = Convert.ToHexString(uint8array.Take(4).ToArray()) + "-" + 
            Convert.ToHexString(uint8array.Take(2).ToArray()) + "-" + 
            Convert.ToHexString(uint8array.Take(2).ToArray()) + "-" + 
            Convert.ToHexString(uint8array.Take(2).ToArray()) + "-" + 
            Convert.ToHexString(uint8array.Take(6).ToArray());


        return result.ToLower();
    }
}
