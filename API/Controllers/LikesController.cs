using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Extentions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{

    [Authorize]
    public class LikesController : BaseApiController
    {

        private readonly IUserRepository _userRepository;
        private readonly ILikesRepository _likesRepository;

        public LikesController(IUserRepository userRepository, ILikesRepository likesRepository)
        {
            _likesRepository = likesRepository;
            _userRepository = userRepository;
        }

        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            var sourceUserId = User.GetUserId();
            var likedUser = await _userRepository.GetUserByNameAsync(username);
            var sourceUser = await _likesRepository.GetUserWithLikes(sourceUserId);

            if (likedUser == null) return NotFound();

            if (sourceUser.UserName == username) return BadRequest("You cannot like your self");

            var userlike = await _likesRepository.GetUserLike(sourceUserId, likedUser.Id);

            if (userlike != null) return BadRequest("you already liked this user");

            userlike = new Entities.UserLike
            {
                SourceUserId = sourceUserId,
                LikedUserId = likedUser.Id
            };

            sourceUser.LikedUsers.Add(userlike);

            if (await _userRepository.SaveAllAsync()) return Ok();

            return BadRequest("failed to like");

        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes([FromQuery] LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();
            var users = await _likesRepository.GetUserLikes(likesParams);

            Response.AddPaginationHeader(users.CurrentPage,
                users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(users);
        }
    }
}