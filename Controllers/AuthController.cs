using Data;
using DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Controllers
{
    [ApiController]
    [Route("Auth/")]
    public class AuthController : ControllerBase
    {

        public readonly IPasswordHasher<User> _hasher;
        public readonly MyDbContext _context;

        public AuthController(MyDbContext context, IPasswordHasher<User> hasher)
        {
            _hasher = hasher;
            _context = context;
        }



        [HttpPost]
        [Route("Registration")]
        public async Task<IActionResult> Registration([FromBody] User user)
        {
            var existUser = await _context.Users.FirstOrDefaultAsync(u => u.NickName == user.NickName);
            if (existUser != null)
                return BadRequest("Пользователь с таким ником уже существует");

            if (string.IsNullOrWhiteSpace(user.NickName) || string.IsNullOrWhiteSpace(user.Password))
                return BadRequest("Имя и пароль обязательны");

            user.Password = _hasher.HashPassword(user, user.Password);

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return Ok($"Пользователь успешно добавлен : {user}");
        }
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] User user)
        {
            var existUser = await _context.Users.FirstOrDefaultAsync(u => u.NickName == user.NickName);

            if (existUser == null)
                return BadRequest("Такого пользователя не существует");

            if (string.IsNullOrWhiteSpace(existUser.Password) || string.IsNullOrWhiteSpace(user.Password))
                return BadRequest("Имя и пароль обязательны");

            var result = _hasher.VerifyHashedPassword(existUser, existUser.Password, user.Password);

            if (result == PasswordVerificationResult.Failed)
                return BadRequest("Пароли Не совпадают");

            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {

                user.Password = _hasher.HashPassword(existUser, user.Password);
                await _context.SaveChangesAsync();
            }
            return Ok($"Успешный вход {user}");

        }
        [HttpPost]
        [Route("Redact/{id}")]
        public async Task<IActionResult> Redact([FromBody] UserChangeRequest redactedUser, [FromRoute] int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return BadRequest("Такого пользователя не существует");

            if (string.IsNullOrWhiteSpace(redactedUser.NewNickname))
                return BadRequest("Никнейм обязателен для изменения");

            if (string.IsNullOrWhiteSpace(redactedUser.Password) || string.IsNullOrWhiteSpace(user.Password))
                return BadRequest("Пароль обязателен и не должен быть пустым");

            var result = _hasher.VerifyHashedPassword(user, user.Password, redactedUser.Password);

            if (result == PasswordVerificationResult.Failed)
                return BadRequest("Пароли не совпадают");

            if (redactedUser.NewNickname == user.NickName)
                return Ok("Никнеймы совпадают, изменения не требуются");

            var existUser = await _context.Users.FirstOrDefaultAsync(u => u.NickName == redactedUser.NewNickname);

            if (existUser != null)
                return BadRequest($"Пользователь {redactedUser.NewNickname} уже существует");

            user.NickName = redactedUser.NewNickname;
            await _context.SaveChangesAsync();
            return Ok($"Пользователь изменен{user}");
        }
        [HttpGet]
        [Route("ShowUser/{id}")]
        public async Task<IActionResult> ShowExactUser([FromRoute] int id)
        {
            var result = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            return result != null ? Ok(result) : BadRequest("Пользователя с такии id не существует");
        }
        [HttpGet]
        [Route("ShowUsers")]
        public async Task<IActionResult> ShowUsers()
        {
            var result = await _context.Users.ToListAsync();
            return Ok(result);
        }
    }

}