namespace FiberEvolutionScraper.Api.Models.User;

using FiberEvolutionScraper.Api.Models.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;

public class User : IBaseModel<int>
{
    public int Id { get; set; }

    [Required]
    public DateTime Created { get; set; }

    [Required]
    public DateTime LastUpdated { get; set; }
}
