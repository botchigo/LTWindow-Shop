using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyShop.Application.Interfaces;
using MyShop.Domain.Entities;
using MyShop.Domain.Interfaces;
using MyShop.Shared.DTOs.Users;

namespace MyShop.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserService> _logger;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, ILogger<UserService> logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<AppUser> LoginAsync(LoginDTO request)
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                throw new Exception("Tên đăng nhập và mật khẩu không được trống");

            var user = await _unitOfWork.Users.GetByUsernameAndPasswordAsync(request.Username, request.Password);
            if (user is null)
                throw new Exception("Tên đăng nhập hoặc mật khẩu không đúng");

            return user;
        }

        public async Task<AppUser> RegisterUserAsync(RegisterDTO request)
        {
            try
            {
                var isExist = await _unitOfWork.Users.IsUsernameExistsAsync(request.Username);

                if (isExist)
                    throw new Exception($"Tên {request.Username} đã tồn tại");

                var newUser = _mapper.Map<AppUser>(request);

                await _unitOfWork.Users.AddAsync(newUser);
                await _unitOfWork.CompleteAsync();

                return newUser;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }
    }
}
