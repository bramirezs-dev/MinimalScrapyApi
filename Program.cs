
using System.Text.Json;
using HtmlAgilityPack;
using System.Net.Cache;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMemoryCache(options => {
    MemoryCacheEntryOptions cacheEntryOptions 
});
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/jujutsu-no-kaisen", GetJujutsuNoKaisen);

app.Run();


static async Task<List<Data>> GetJujutsuNoKaisen(IMemoryCache memory){
    

    var stringHtml = await GetToHtml("https://jujutsu-kaisen.fandom.com/es/wiki/Lista_de_Personajes#Manga");
    var nodesHtml  = GetInformationScrapy(stringHtml,"//div[@class='tabbig']/div/div[@class='wds-tab__content wds-is-current']");
    
    var response = new List<Data>();

    var cleanData = nodesHtml[0].ChildNodes.Where( child=> child.Name != "#text" && child.Name != "p" ).ToList();

    var result = cleanData.Select((x,y)=> {
        if(x.Name == "table" && cleanData[y+1].Name == "table"){
            return new {index=y , header=true};
        }
        return new {index=y , header=false};
    }).Where(y=> y.header == true).ToList();

    for (int i = 0; i < result.Count ; i++)
    {
        if( i == (result.Count - 1)){
            var range = cleanData.Skip(result[i].index).ToList();
            var data = Valid(range);
            response.Add(data);
        }else{
            var range = cleanData.Skip(result[i].index).Take(result[i + 1].index).ToList();
            var data = Valid(range);
            response.Add(data);
        }
        
    }
    var cacheEntryOptions = new MemoryCacheEntryOptions().AbsoluteExpiration=DateTime.Now.AddMinutes(1);¡
    memory.Set<List<Data>>("hierarchy",response);
    
    return response;
}


static async Task<string> GetToHtml(string url)
{

    var client = new HttpClient();
    var response = await client.GetStringAsync(url);
    return response;
}

static HtmlNodeCollection GetInformationScrapy(string page,string queryXpath){
    var doc = new HtmlDocument();
    doc.LoadHtml(page);
    var queryHtmlNodeCollection = doc.DocumentNode.SelectNodes(queryXpath);
    return queryHtmlNodeCollection;
}

static Data Valid (List<HtmlNode> node, int index = 0){
     var result = new Data();
     result.Secondaries = new List<Secondary>();
     result.Main = node[index].InnerText.Replace("\n",String.Empty);

     var secondary = String.Empty;
     foreach (var item in node.SkipLast(index))
     {
        if(item.Name == "table"){
            secondary = item.InnerText.Replace("\n",String.Empty);
            continue;
        }
        if(item.Name == "div"){
            var characters = GetInformationScrapy(item.InnerHtml,"//a")
                             .Where(i => i.InnerText != "")
                             .Select( x=> new Character{ Route = x.Attributes["href"].Value, Name = x.InnerText})
                             .ToList();
            result.Secondaries.Add(new Secondary{
                Name = secondary,
                Characters = characters
            });
        }
     }
     return result;    
}


static void AddCache<T>(T data, string key) {
    var cacheEntryOptions = new MemoryCacheEntryOptions().AbsoluteExpiration=DateTime.Now.AddMinutes(1);¡
    memory.Set<List<T>>("hierarchy",data);
}
class Data {
    public string  Main { get; set; }

    public List<Secondary> Secondaries { get; set; }

    
}

class Secondary {
    public string  Name { get; set; }

    public List<Character> Characters {get;set;}
}

class Character {
    public string Name { get; set; }

    public string Route { get; set; }
}


