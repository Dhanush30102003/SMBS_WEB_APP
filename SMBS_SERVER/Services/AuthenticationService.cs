using SMBS_SERVER.Models.Masters;
using SMBS_SERVER.Repositories;

namespace SMBS_SERVER.Services
{
    public class AuthenticationService
    {
        private readonly UserRepository _repo;

        public AuthenticationService(UserRepository repo)
        {
            _repo = repo;
        }

        public AuthenticationResult Login(string username, string password, int tenantId)
        {
            // 1️⃣ Basic input validation
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "Username and password are required."
                };
            }

            // 2️⃣ Ask repository to authenticate
            var user = _repo.Authenticate(username, password, tenantId);

            // 3️⃣ Handle invalid login
            if (user == null)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "Invalid username or password."
                };
            }

            // 4️⃣ Handle locked user (optional but recommended)
            if (user.IsLocked)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "User account is locked."
                };
            }

            // 5️⃣ SUCCESS
            return new AuthenticationResult
            {
                Success = true,
                User = user,
                Message = "Login successful."
            };
        }
    }

    public class AuthenticationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public UserMaster User { get; set; }
    }
}
