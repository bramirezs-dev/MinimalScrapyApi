namespace MinimalScrapyApi.DTOs
{
    public class JujutsuKaisenDTO
    {
        public string  MainCategory { get; set; }

        public List<CategoryDTO> Categories { get; set; }
    }
}