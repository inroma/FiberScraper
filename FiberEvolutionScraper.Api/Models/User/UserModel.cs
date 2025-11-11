namespace FiberEvolutionScraper.Api.Models.User;

public class UserModel : BaseModel
{
    public string UserName { get; set; }

    public string UId { get; set; }

    public virtual ICollection<AutoRefreshInput> AutoRefreshs { get; set; }
}
