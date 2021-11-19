namespace APIDemo.Models.Users
{
    public record UserDto(string UserName, string Password);

    public record UserModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public interface IUserRepositoryService
    {
        UserDto GetUser(UserModel userModel);
    }
    public class UserRepositoryService : IUserRepositoryService
    {
        private List<UserDto> _users => new()
        {
            new("admin", "abc123"),

        };
        public UserDto GetUser(UserModel userModel)
        {
            return _users.FirstOrDefault(x => string.Equals(x.UserName, userModel.UserName) && string.Equals(x.Password, userModel.Password));
        }
    }
}
