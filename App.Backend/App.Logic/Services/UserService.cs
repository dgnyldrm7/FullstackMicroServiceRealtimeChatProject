using App.Core.DTOs;
using App.Core.Entities;
using App.Core.Interface;
using App.Core.Result;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace App.Logic.Services
{
    public class UserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public UserService(
            UserManager<AppUser> userManager,
            IUnitOfWork unitOfWork,
            RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            _unitOfWork = unitOfWork;
            this.roleManager = roleManager;
        }

        /// <summary>
        /// Tüm kullanıcıları getirir.
        /// </summary>
        public async Task<Result<List<AppUser>>> GetAllUsersAsync()
        {
            List<AppUser> users = await userManager.Users.ToListAsync();

            if(users == null)
            {
                return Result<List<AppUser>>.Failure("No users found", 404);
            }

            if(users.Count < 1)
            {
                return Result<List<AppUser>>.Failure("No users found", 404);
            }

            return Result<List<AppUser>>.Success(users, 200);
        }

        /// <summary>
        ///   User Kayıt Etme Metodudur. User nesnesi ve şifresi alır.
        /// </summary>
        public async Task<Result<IdentityResult>> RegisterUserAsync(CreateUserDto user)
        {
            AppUser? hasUser = await userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == user.PhoneNumber);

            AppUser? hasUserByEmail = await userManager.Users.FirstOrDefaultAsync(x => x.Email == user.Email);

            if (hasUser != null && hasUserByEmail != null)
            {
                return Result<IdentityResult>.Failure("User already exists, the same number or mail cannot be used.", 409);
            }

            AppUser newUser = new AppUser
            {
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
            };

            IdentityResult result = await userManager.CreateAsync(newUser, user.Password!);

            if (result.Succeeded)
            {
                await _unitOfWork.SaveChangesAsync(CancellationToken.None);
            }
            else
            {
                List<string> errorMessages = result.Errors.Select(e => e.Description).ToList();

                string fullErrorMessage = string.Join(" | ", errorMessages);

                return Result<IdentityResult>.Failure(fullErrorMessage, 400);
            }

            // Kullanıcıya rol atama
            // Eğer "User" rolü yoksa oluştur
            var hasRole = await roleManager.RoleExistsAsync("User");

            if (!hasRole)
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }

            // Kullanıcıya "User" rolünü ata
            await userManager.AddToRoleAsync(newUser, "User");

            return Result<IdentityResult>.Success(result, 201);
        }

        /// <summary>
        ///   User'ı Id'sine göre bulur.
        /// </summary>
        public async Task<Result<UserDto>> GetUserByIdAsync(string userId)
        {
            AppUser? user = await userManager.FindByIdAsync(userId);

            UserDto data = new UserDto
            {
                Id = user?.Id,
                UserName = user?.UserName,
                Email = user?.Email,
                PhoneNumber = user?.PhoneNumber
            };

            if (user == null)
            {
                return Result<UserDto>.Failure("User not found", 404);
            }

            return Result<UserDto>.Success(data, 200);

        }

        /// <summary>
        ///     User'on numarasına göre bulma işlemidir!
        /// </summary>
        public async Task<Result<UserDto>> GetUserByNumber(string userNumber)
        {
            AppUser? user = await userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == userNumber);

            if(user == null)
            {
                return Result<UserDto>.Failure("User not found", 404);
            }

            UserDto resultUserDto = new UserDto
            {
                Id= user?.Id,
                Email = user?.Email,
                UserName = user?.UserName,
                PhoneNumber = user?.PhoneNumber
            };

            return Result<UserDto>.Success(resultUserDto, 200);
        }

        /// <summary>
        ///   User'ın Emailine göre bulur.
        /// </summary>
        public async Task<Result<AppUser>> GetUserByEmail(string email)
        {
            AppUser? user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return Result<AppUser>.Failure("User not found", 404);
            }

            return Result<AppUser>.Success(user, 200);
        }
    }
}
