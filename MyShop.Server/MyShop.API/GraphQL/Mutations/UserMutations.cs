using MyShop.Application.Commons;
using MyShop.Application.Interfaces;
using MyShop.Domain.Entities;
using MyShop.Shared.DTOs.Users;

namespace MyShop.API.GraphQL.Mutations
{
    [ExtendObjectType("Mutation")]
    public class UserMutations
    {
        public async Task<TPayload<LoginResponse>> LoginAsync(
            [Service] IUserService userService,
            LoginDTO request)
        {
            try
            {
                var response = await userService.LoginAsync(request);
                return new TPayload<LoginResponse>(response);
            }
            catch(Exception ex)
            {
                return new TPayload<LoginResponse>(ex.Message);
            }
        }

        public async Task<TPayload<AppUser>> RegisterAsync(
            [Service] IUserService userService,
            RegisterDTO request)
        {
            try
            {
                var user = await userService.RegisterUserAsync(request);
                return new TPayload<AppUser>(user);
            }
            catch(Exception ex)
            {
                return new TPayload<AppUser>(ex.Message);
            }
        }
    }
}
