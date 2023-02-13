﻿using System.ComponentModel.DataAnnotations;

namespace Blog.ViewModels.Accounts;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Nome obrigatorio")]
    public string Name { get; set; }
    
    [Required(ErrorMessage = "Email obrigatorio")]
    [EmailAddress(ErrorMessage = "Email invalido")]
    public string Email { get; set; }
}