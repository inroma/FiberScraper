namespace FiberEvolutionScraper.Api.Managers;

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
}
