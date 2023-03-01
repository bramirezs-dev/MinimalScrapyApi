namespace MinimalScrapyApi.DTOs
{
    public class CategoryDTO
    {
        public string  Name { get; set; }

        public List<CharacterDTO> Characters {get;set;}
    }
}