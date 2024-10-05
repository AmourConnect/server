﻿using Domain.Dtos.GetDtos;
using Domain.Entities;
using Infrastructure.Interfaces;

namespace Infrastructure.DistributedCaching
{
    public class UserCaching(IUserRepository userRepository, ICacheService CacheService) : IUserCaching
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly ICacheService _cacheService = CacheService;
        public async Task<User> GetUserWithCookieAsync(string token_session_user)
        {
            User userCache = await _cacheService.GetAsync<User>(token_session_user.ToString());
            if (userCache is null)
            {
                User user = await _userRepository.GetUserWithCookieAsync(token_session_user.ToString());

                await _cacheService.SetAsync(token_session_user.ToString(), user, TimeSpan.FromSeconds(30));

                return user;
            }
            return userCache;
        }

        public async Task<ICollection<GetUserDto>> GetUsersToMatchAsync(User dataUserNowConnect)
        {
            ICollection<GetUserDto> UsersCache = await _cacheService.GetAsync<ICollection<GetUserDto>>(dataUserNowConnect.Id_User.ToString());

            if (UsersCache is null)
            {
                ICollection<GetUserDto> getUserDto = await _userRepository.GetUsersToMatchAsync(dataUserNowConnect);

                await _cacheService.SetAsync(dataUserNowConnect.Id_User.ToString(), getUserDto, TimeSpan.FromSeconds(60));

                return getUserDto;
            }

            return UsersCache;
        }

        public async Task<User> GetUserByIdUserAsync(int Id_User)
        {
            User userCache = await _cacheService.GetAsync<User>(Id_User.ToString());
            if(userCache is null)
            {
                User user = await _userRepository.GetUserByIdUserAsync(Id_User);

                await _cacheService.SetAsync(Id_User.ToString(), user, TimeSpan.FromSeconds(15));

                return user;
            }
            return userCache;
        }
    }
}