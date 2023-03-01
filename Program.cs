
using System.Text.Json;
using HtmlAgilityPack;
using System.Net.Cache;
using Microsoft.Extensions.Caching.Memory;
using MinimalScrapyApi.DTOs;
using MinimalScrapyApi.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMemoryCache();
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/jujutsu-no-kaisen", GetJujutsuNoKaisen);

app.Run();


static async Task<List<JujutsuKaisenDTO>> GetJujutsuNoKaisen(IMemoryCache memory){
    
    var existInMemory = memory.Get<List<JujutsuKaisenDTO>>("hierarchy");
    if (existInMemory != null)
    {
        return existInMemory;
    }

    var stringHtml = await GetToHtml("https://jujutsu-kaisen.fandom.com/es/wiki/Lista_de_Personajes#Manga");
    var nodesHtml  = GetInformationScrapy(stringHtml,"//div[@class='tabbig']/div/div[@class='wds-tab__content wds-is-current']");
    
    var response = new List<JujutsuKaisenDTO>();

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
    
    memory.AddKey<List<JujutsuKaisenDTO>>("hierarchy",response,DateTime.Now.AddMinutes(1));
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

static JujutsuKaisenDTO Valid (List<HtmlNode> node, int index = 0){
     var result = new JujutsuKaisenDTO();
     result.Categories = new List<CategoryDTO>();
     result.MainCategory = node[index].InnerText.Replace("\n",String.Empty);

     var category = String.Empty;
     foreach (var item in node.SkipLast(index))
     {
        if(item.Name == "table"){
            category = item.InnerText.Replace("\n",String.Empty);
            continue;
        }
        if(item.Name == "div"){
            var characters = GetInformationScrapy(item.InnerHtml,"//a")
                             .Where(i => i.InnerText != "")
                             .Select( x=> new CharacterDTO{ Route = x.Attributes["href"].Value, Name = x.InnerText})
                             .ToList();
            result.Categories.Add(new CategoryDTO{
                Name = category,
                Characters = characters
            });
        }
     }
     return result;    
}


