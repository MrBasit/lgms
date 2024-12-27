using LGMS.Data.Context;
using LGMS.Data.Model;
using LGMS.Data.Models.Authentication;
using LGMS.Dto;
using MailSender.Model;
using MailSender.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace LGMS.Controllers
{
    [Authorize(Roles = "Access")]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly LgmsDbContext _context;
        private readonly SignInManager<IdentityUser> _signInManager;
        public AuthenticationController
        (
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<IdentityUser> signInManager,
            IConfiguration configuration,
            IEmailService emailService,
            LgmsDbContext context
        )
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
            _emailService = emailService;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("RegisterUser")]
        public async Task<IActionResult> RegisterUser(RegisterUser registerUser)
        {
            var employee = new Employee();
            if (string.IsNullOrEmpty(registerUser.Email) || string.IsNullOrEmpty(registerUser.Username)) {
                return StatusCode(StatusCodes.Status400BadRequest, new Response
                {
                    Status = "Failed",
                    Message = "user email and name is required"
                });
            }

            var isUniqueUserEmail = await _userManager.FindByEmailAsync(registerUser.Email) == null;
            var isUniqueUsername = await _userManager.FindByNameAsync(registerUser.Username) == null;

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

            if (registerUser.EmployeeId > 0)
            {
                employee = _context.Employees.Include(e=>e.IdentityUser).SingleOrDefault(e => e.Id == registerUser.EmployeeId);
                if (employee == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new Response
                    {
                        Status = "Failed",
                        Message = "Employee with provided id not found"
                    });
                }
                if (employee.IdentityUser != null) 
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new Response
                    {
                        Status = "Failed",
                        Message = string.Format("One user account with username '{0}' is already associated with entered employee", employee.IdentityUser.UserName)
                    });
                }
            }
            else if (registerUser.Employee != null) 
            {
                return BadRequest("Code Not Implemented");
                //create employee and retreive Employee
            }
            else 
            {
                return StatusCode(StatusCodes.Status400BadRequest, new Response
                {
                    Status = "Failed",
                    Message = "Employee Id or Employee Information is required."
                });
            }
            

            if (isUniqueUserEmail && isUniqueUsername)
            {
                var user = new IdentityUser
                {
                    Email = registerUser.Email,
                    UserName = registerUser.Username,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    TwoFactorEnabled = true,
                    LockoutEnabled = false,
                    EmailConfirmed = true
                };

                var generatedPassword = GeneratePassword();
                registerUser.Password = generatedPassword;

                var result = await _userManager.CreateAsync(user, registerUser.Password);

                if (result.Succeeded)
                {
                    try
                    {
                        //assign user role if exist
                        var rolestore = new RoleStore<IdentityRole>(_context);
                        var emprole = rolestore.Roles.SingleOrDefault(r=>r.Name == "Employee");
                        if (emprole != null)
                        {
                            var a = await _userManager.AddToRoleAsync(user, emprole.Name);
                        }

                        //add User to employee
                        employee.IdentityUser = user;
                        _context.Employees.Update(employee);
                        _context.SaveChanges();

                        //send email to user email address containing username, email and password for the portal
                        var messageContent = string.Format("Username: {0}\nEmail: {1}\nPassword:{2}", registerUser.Username, registerUser.Email, registerUser.Password);
                        var message = new Message(new string[] { registerUser.Email }, "Welcome to LGMS, Your portal login information", messageContent);
                        _emailService.SendEmail(message);
                    }
                    catch (Exception ex)
                {
                        return StatusCode(StatusCodes.Status201Created, new Response
                        {
                            Status = "PartialSucceeded",
                            Message = "User created successfully but not attached with Employee"
                });

            }

                    return StatusCode(StatusCodes.Status201Created, new Response
                {
                        Status = "Succeeded",
                        Message = "User created successfully and attached with Employee."
                });
                }

                return StatusCode(StatusCodes.Status500InternalServerError, new Response
                {
                    Status = "Failed",
                    Message = result.Errors.First().Description
                });
            }

            return StatusCode(StatusCodes.Status400BadRequest, new Response
            {
                Status = "Failed",
                Message = "User registration failed."
            });
        }

        [HttpGet("GetUserPermissions")]
        public async Task<IActionResult> GetUserPermissions(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(roles);
        }

        [HttpGet("GetPermissions")]
        public async Task<IActionResult> GetPermissions()
        {
            var rolestore = new RoleStore<IdentityRole>(_context);
            var roles = rolestore.Roles.Select(r=>r.NormalizedName).ToList();
            return Ok(roles);
        }

        [HttpPost("GrantPermission")]
        public async Task<IActionResult> GrantPermission([FromBody] GrantPermissionModel permissionModel)
        {
            var rolestore = new RoleStore<IdentityRole>(_context);
            var user = await _userManager.FindByNameAsync(permissionModel.Username);
            if (user == null) return BadRequest("User Not Found");

            var roles = rolestore.Roles.Where(r => permissionModel.Roles.Contains(r.NormalizedName)).ToList();

            if (roles.Count != permissionModel.Roles.Length)
            {
                var rolesNotFound = permissionModel.Roles.Where(r => !roles.Select(rr => rr.NormalizedName).Contains(r)).ToList();
                if (rolesNotFound.Count > 0)
                {
                    return BadRequest(string.Format("Role(s) with name [{0}] not found", string.Join(", ", rolesNotFound)));
                }
            }

            var existingRoles = await _userManager.GetRolesAsync(user);

            var rolesToKeep = existingRoles.Intersect(roles.Select(r => r.Name)).ToList();

            var rolesToRemove = existingRoles.Except(roles.Select(r => r.Name)).ToList();

            var rolesToAdd = roles.Select(r => r.Name).Except(existingRoles).ToList();

            var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded)
            {
                return BadRequest("Failed to remove existing roles");
            }

            var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
            if (!addResult.Succeeded)
            {
                return BadRequest("Failed to assign new roles");
            }

            return Ok();
        }


        [AllowAnonymous]
        [HttpPost("UserLogin")]
        public async Task<IActionResult> UserLogin([FromBody] UserLogin userLogin)
        {
            var user = await _userManager.FindByEmailAsync(userLogin.Username);
            user = user == null ? await _userManager.FindByNameAsync(userLogin.Username) : user;

            if (user != null && (await _userManager.CheckPasswordAsync(user, userLogin.Password)))
            {
                await _signInManager.SignOutAsync();
                await _signInManager.PasswordSignInAsync(user, userLogin.Password, false, true);
                await _signInManager.GetTwoFactorAuthenticationUserAsync();
                var TwoFA = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
                var message = new Message(new string[] { user.Email }, "Project Progreses - OTP", $"Your login OTP is {TwoFA}");

                return await UserLogin2FA(userLogin.Username, TwoFA,true);

                _emailService.SendEmail(message);
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "Signed In, Please continue login with OTP sent to your email" });


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

        [AllowAnonymous]
        [HttpPost("UserLogin2FA")]
        public async Task<IActionResult> UserLogin2FA(string Username, string OTP, bool rememberMe)
        {
            //check username and password
            var user = await _userManager.FindByEmailAsync(Username);
            user = user == null ? await _userManager.FindByNameAsync(Username) : user;
            var SignIn = await _signInManager.TwoFactorSignInAsync("Email", OTP, false, false);
            if (user != null && SignIn.Succeeded)
            {
                //maintain list of claims
                var authClaims = new List<Claim>() {
                    new Claim(ClaimTypes.Name,Username),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
                };
                var userRoles = await _userManager.GetRolesAsync(user);
                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var loggedInUserEmployee = _context.Employees.Include(e => e.IdentityUser).SingleOrDefault(e=>e.IdentityUser.Id == user.Id);
                authClaims.Add(new Claim(type:"EmployeeId",value:loggedInUserEmployee != null ? loggedInUserEmployee.Id.ToString() : ""));
                authClaims.Add(new Claim(type: "EmployeeName", value: loggedInUserEmployee != null ? loggedInUserEmployee.Name.ToString() : ""));
                authClaims.Add(new Claim(type:"EmployeeNumber",value:loggedInUserEmployee != null ? loggedInUserEmployee.EmployeeNumber : ""));
                authClaims.Add(new Claim(type: "Email", value: loggedInUserEmployee.IdentityUser != null ? loggedInUserEmployee.IdentityUser.Email : ""));
                authClaims.Add(new Claim(type: "Username", value: loggedInUserEmployee != null ? loggedInUserEmployee.IdentityUser.UserName : ""));
                //generate the token
                var token = GetJwtToken(authClaims, rememberMe);

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                });
            }
            return Unauthorized();
        }

        [AllowAnonymous]
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new Response
                {
                    Status = "Failed",
                    Message = "Token is required"
                });
            }

            var user = GetUserFromToken(token); 
            if (user == null)
            {
                return Unauthorized(new Response
                {
                    Status = "Failed",
                    Message = "Invalid token"
                });
            }

            var isValidOldPassword = await _userManager.CheckPasswordAsync(user, request.OldPassword);
            if (!isValidOldPassword)
            {
                return BadRequest(new Response
                {
                    Status = "Failed",
                    Message = "Incorrect old password"
                });
            }

            if (request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest(new Response
                {
                    Status = "Failed",
                    Message = "New password and confirm password do not match"
                });
            }

            var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);

            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response
                {
                    Status = "Failed",
                    Message = result.Errors.First().Description
                });
            }

            return Ok(new Response
            {
                Status = "Succeeded",
                Message = "Password changed successfully"
            });
        }

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

        //[AllowAnonymous]
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

        private IdentityUser GetUserFromToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var userId = jwtToken?.Claims?.FirstOrDefault(c => c.Type == "Username")?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return null;
                }

                var user = _userManager.FindByNameAsync(userId).Result;
                return user;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private JwtSecurityToken GetJwtToken(List<Claim> claims, bool rememberMe=false)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: rememberMe ? DateTime.Now.AddDays(7) : DateTime.Now.AddHours(9),
                claims: claims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );
            return token;
        }

        private static string GeneratePassword()
        {
            const string uppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowercaseLetters = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string specialCharacters = "!@#$%^&*()_+-=[]{}|;:,.<>?";
            const int passwordLength = 10;

            Random random = new Random();

            // Ensure at least one of each required character type
            char[] password = new char[passwordLength];
            password[0] = uppercaseLetters[random.Next(uppercaseLetters.Length)];
            password[1] = lowercaseLetters[random.Next(lowercaseLetters.Length)];
            password[2] = digits[random.Next(digits.Length)];
            password[3] = specialCharacters[random.Next(specialCharacters.Length)];

            // Fill the rest of the password with random characters from all categories
            string allCharacters = uppercaseLetters + lowercaseLetters + digits + specialCharacters;
            for (int i = 4; i < passwordLength; i++)
            {
                password[i] = allCharacters[random.Next(allCharacters.Length)];
            }

            // Shuffle the password to randomize character positions
            return new string(password.OrderBy(_ => random.Next()).ToArray());
        }
    }
}
