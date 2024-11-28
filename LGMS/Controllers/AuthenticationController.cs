using LGMS.Data.Context;
using LGMS.Data.Models.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using LGMS.Data.Model;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace LGMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly LgmsDbContext _context;
        private readonly SignInManager<IdentityUser> _signInManager;
        public AuthenticationController
        (
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<IdentityUser> signInManager,
            IConfiguration configuration,
            LgmsDbContext context
        )
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("RegisterUser")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUser registerUser)
        {
            var isUniqueUserEmail = await _userManager.FindByEmailAsync(registerUser.Email) == null;
            var isUniqueUsername = await _userManager.FindByNameAsync(registerUser.Username) == null;

            if (isUniqueUserEmail && isUniqueUsername)
            {
                var user = new IdentityUser
                {
                    Email = registerUser.Email,
                    UserName = registerUser.Username,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, registerUser.Password);

                if (result.Succeeded)
                {
                    return StatusCode(StatusCodes.Status201Created, new Response
                    {
                        Status = "Succeeded",
                        Message = "User created successfully."
                    });
                }

                return StatusCode(StatusCodes.Status500InternalServerError, new Response
                {
                    Status = "Failed",
                    Message = result.Errors.First().Description
                });
            }

            if (!isUniqueUserEmail)
                return StatusCode(StatusCodes.Status400BadRequest, new Response
                {
                    Status = "Failed",
                    Message = "User with this email already exists."
                });

            if (!isUniqueUsername)
                return StatusCode(StatusCodes.Status400BadRequest, new Response
                {
                    Status = "Failed",
                    Message = "User with this username already exists."
                });

            return StatusCode(StatusCodes.Status400BadRequest, new Response
            {
                Status = "Failed",
                Message = "User registration failed."
            });
        }



        [HttpPost("UserLogin")]
        public async Task<IActionResult> UserLogin([FromBody] UserLogin userLogin)
        {
            var user = await _userManager.FindByEmailAsync(userLogin.Username);
            user = user == null ? await _userManager.FindByNameAsync(userLogin.Username) : user;

            if (user != null && (await _userManager.CheckPasswordAsync(user, userLogin.Password)))
            {
                var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, userLogin.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

                var userRoles = await _userManager.GetRolesAsync(user);
                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var token = GetJwtToken(authClaims);

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    userId = user.Id,
                    validity = token.ValidTo
                });
            }
            return Unauthorized();
        }

        //[HttpPost("UserLogin2FA")]
        //public async Task<IActionResult> UserLogin2FA(string Username, string OTP, bool rememberMe)
        //{
        //    //check username and password
        //    var user = await _userManager.FindByEmailAsync(Username);
        //    user = user == null ? await _userManager.FindByNameAsync(Username) : user;
        //    var SignIn = await _signInManager.TwoFactorSignInAsync("Email", OTP, false, false);
        //    if (user != null && SignIn.Succeeded)
        //    {
        //        //maintain list of claims
        //        var authClaims = new List<Claim>() {
        //            new Claim(ClaimTypes.Name,Username),
        //            new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
        //        };
        //        var userRoles = await _userManager.GetRolesAsync(user);
        //        foreach (var userRole in userRoles)
        //        {
        //            authClaims.Add(new Claim(ClaimTypes.Role, userRole));
        //        }

        //        //generate the token
        //        var token = GetJwtToken(authClaims, rememberMe);

        //        return Ok(new
        //        {
        //            token = new JwtSecurityTokenHandler().WriteToken(token),
        //            userId = user.Id,
        //            validity = token.ValidTo
        //        });
        //    }
        //    return Unauthorized();
        //}


        //[HttpPost("ResendOTP")]
        //public async Task<IActionResult> ResendOTP([FromBody] ResendOtpRequest request)
        //{
        //    var user = await _userManager.FindByEmailAsync(request.Username) ?? await _userManager.FindByNameAsync(request.Username);

        //    if (user == null)
        //    {
        //        return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Failed", Message = "User not found" });
        //    }

        //    var otp = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
        //    var message = new Message(new string[] { user.Email }, "Project Progress - OTP Resend", $"Your new login OTP is {otp}");

        //    _emailService.SendEmail(message);

        //    return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "New OTP sent successfully to your email." });
        //}

        //[HttpGet("ConfirmEmail")]
        //public async Task<IActionResult> ConfirmEmail(string token, string email)
        //{
        //    var user = await _userManager.FindByEmailAsync(email);
        //    if (user != null)
        //    {
        //        var result = await _userManager.ConfirmEmailAsync(user, token);
        //        if (result.Succeeded) { return StatusCode(StatusCodes.Status200OK, new Response { Message = "Verification Email Successfully", Status = "Success" }); }
        //        return StatusCode(StatusCodes.Status500InternalServerError, new Response { Message = "Something went wrong while verifying email", Status = "Success" });
        //    }
        //    return StatusCode(StatusCodes.Status404NotFound, new Response { Message = "User with this email not found", Status = "Failed" });
        //}

        //[HttpPost("ForgetPassword")]
        //public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordRequest request)
        //{
        //    var user = await _userManager.FindByEmailAsync(request.Email);

        //    if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
        //    {
        //        return BadRequest(new { message = "User not found or email not confirmed." });
        //    }

        //    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        //    var baseurl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
        //    var resetLink = $"{baseurl}/#/reset-password?token={Uri.EscapeDataString(token)}&email={user.Email}";

        //    var message = new Message(new string[] { user.Email }, "Reset Password - Project Progress", resetLink);
        //    _emailService.SendEmail(message);

        //    return Ok(new Response { Status = "Success", Message = "Password reset link has been sent to your email." });
        //}

        //[HttpPost("ResetPassword")]
        //public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        //{
        //    var user = await _userManager.FindByEmailAsync(request.Email);
        //    if (user == null)
        //    {
        //        return BadRequest(new { message = "User not found." });
        //    }

        //    var resetCode = WebUtility.UrlDecode(request.ResetCode);

        //    var result = await _userManager.ResetPasswordAsync(user, resetCode, request.NewPassword);
        //    if (result.Succeeded)
        //    {
        //        return Ok(new Response { Status = "Success", Message = "Password has been reset successfully." });
        //    }

        //    return BadRequest(new { message = "Error resetting password: " + string.Join(", ", result.Errors.Select(e => e.Description)) });
        //}

        //[HttpGet("TestEmail")]
        //public async Task<IActionResult> TestEmail()
        //{
        //    var message = new Message(new string[] { "logicade.domains@gmail.com" }, "Test Email from Project Progress", "<h1>Welcome To Project Progress</h1>");
        //    _emailService.SendEmail(message);
        //    return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "Email Sent Succesfully" });
        //}


        private JwtSecurityToken GetJwtToken(List<Claim> claims, bool rememberMe=false)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            var token = new JwtSecurityToken(
               expires: rememberMe ? DateTime.Now.AddDays(7) : DateTime.Now.AddHours(9),
                claims: claims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );
            return token;
        }
    }
}
