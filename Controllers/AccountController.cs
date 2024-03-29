﻿using System.Text.RegularExpressions;
using Blog.Data;
using Blog.Extensions;
using Blog.Models;
using Blog.Services;
using Blog.ViewModels;
using Blog.ViewModels.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureIdentity.Password;

namespace Blog.Controllers;

[ApiController]
public class AccountController : ControllerBase
{
    private readonly TokenService _tokenService;

    public AccountController(TokenService tokenService)
    {
        _tokenService = tokenService;
    }

    [HttpPost("v1/accounts")]
    public async Task<IActionResult> Post(
        [FromBody] RegisterViewModel model,
        [FromServices] EmailService emailService,
        [FromServices] BlogDataContext context)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ResultViewModel<string>(ModelState.GetErros()));
        }

        var user = new User()
        {
            Name = model.Name,
            Email = model.Email,
            Slug = model.Email.Replace("@", "_").Replace(".", "-")
        };

        var password = PasswordGenerator.Generate(25);
        user.PasswordHash = PasswordHasher.Hash(password);
        
        

        try
        {
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            emailService.Send(
                user.Name, 
                user.Email, 
                "Bem vindo ao teste", 
                $"Sua senha é {password}");

            return Ok(new ResultViewModel<dynamic>(new
            {
                user = user.Email, password
            }));
        }
        catch (DbUpdateException)
        {
            return StatusCode(400, new ResultViewModel<string>("Usuario ja cadastrado"));
        }
        
        catch 
        {
            return StatusCode(500, new ResultViewModel<string>("Server Error"));
        }
    }
    
    [HttpPost("v1/login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginViewModel model,
        [FromServices] BlogDataContext context)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ResultViewModel<string>(ModelState.GetErros()));
        }

        var user = await context
            .Users
            .AsNoTracking()
            .Include(x => x.Roles)
            .FirstOrDefaultAsync(x => x.Email == model.Email);

        if (user == null)
            return StatusCode(401, new ResultViewModel<string>("Usuario ou senha invalido"));
        
        if(!PasswordHasher.Verify(user.PasswordHash, model.Password))
            return StatusCode(401, new ResultViewModel<string>("Usuario ou senha invalido"));

        try
        {
            var token = _tokenService.GenerateToken(user);
            return Ok(new ResultViewModel<string>(token, null));
        }
        catch
        {
            return StatusCode(500, new ResultViewModel<string>("Server error"));
        }
    }

    [Authorize]
    [HttpPost("v1/accounts/upload-image")]
    public async Task<IActionResult> UploadImage(
        [FromBody] UploadImageViewModel model,
        [FromServices] BlogDataContext context)
    {
        var fileName = $"{Guid.NewGuid().ToString()}.jpg";
        var data = new Regex(@"^data:image\/[a-z]+;base64,").Replace(model.Base64Image, "");
        var bytes = Convert.FromBase64String(data);

        try
        {
            await System.IO.File.WriteAllBytesAsync($"Wwwroot/Images/{fileName}", bytes);
        }
        catch (Exception e)
        {
            return StatusCode(500, new ResultViewModel<string>("Falha interna"));
        }

        var user = await context.Users.FirstOrDefaultAsync(x => x.Email == User.Identity.Name);

        if (user == null)
        {
            return NotFound(new ResultViewModel<User>("Usuario não encontrado"));
        }

        user.Image = $"https://localhost:0000/images/{fileName}";
        
        try
        {
            context.Users.Update(user);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ResultViewModel<string>("Falha interna"));
        }

        return Ok(new ResultViewModel<string>("Imagem alterada com sucesso!", null));

    }
}