using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Br.DollarQuotation.Application.DTOs.Requests
{
    public sealed class RegisterUserRequest
    {
        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string ConfirmPassword { get; set; } = string.Empty;

        public string Role { get; set; } = "User";

        public string? PhotoBase64 { get; set; }

        public string? PhotoContentType { get; set; }
    }
}
