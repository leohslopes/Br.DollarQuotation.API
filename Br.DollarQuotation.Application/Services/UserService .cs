using Br.DollarQuotation.Application.DTOs.Requests;
using Br.DollarQuotation.Application.DTOs.Responses;
using Br.DollarQuotation.Application.Interfaces.Services;
using Br.DollarQuotation.Domain.Entities;
using Br.DollarQuotation.Domain.Exceptions;
using Br.DollarQuotation.Domain.Interfaces.Repositories;
using Br.DollarQuotation.Domain.Interfaces.Services;
using Br.DollarQuotation.Domain.ValueObjects;


namespace Br.DollarQuotation.Application.Services
{
    public sealed class UserService : IUserService
    {
        private const int MinimumPasswordLength = 8;
        private const int MaximumPasswordLength = 100;

        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public UserService(IUserRepository userRepository,
                           IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<RegisterUserResponse> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
        {
            ValidateRequest(request);
            ValidatePassword(request.Password, request.ConfirmPassword);

            var email = Email.Create(request.Email);
            var emailAlreadyExists = await _userRepository.EmailExistsAsync(email, cancellationToken);

            if (emailAlreadyExists)
                throw new EmailAlreadyRegisteredException(email.Value);

            var passwordHash = _passwordHasher.Hash(request.Password);
            var user = new User(name: request.Name,
                email: email,
                passwordHash: passwordHash,
                photoBase64: request.PhotoBase64,
                photoContentType: request.PhotoContentType);

            await _userRepository.AddAsync(user, cancellationToken);

            return MapToResponse(user);
        }

        public async Task<UserResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty)
            {
                throw new DomainException("O identificador do usuário é obrigatório.");
            }

            var user = await _userRepository.GetByIdAsync(id, cancellationToken);

            if (user is null)
            {
                throw new UserNotFoundException(id);
            }

            return MapToUserResponse(user);
        }

        public async Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty)
            {
                throw new DomainException( "O identificador do usuário é obrigatório.");
            }

            if (request is null)
            {
                throw new DomainException("Os dados do usuário são obrigatórios.");
            }

            var user = await _userRepository.GetByIdAsync(id, cancellationToken);

            if (user is null)
            {
                throw new UserNotFoundException(id);
            }

            var email = Email.Create(request.Email);

            if (user.Email != email)
            {
                var emailAlreadyExists = await _userRepository.EmailExistsAsync(email, cancellationToken);

                if (emailAlreadyExists)
                {
                    throw new EmailAlreadyRegisteredException(email.Value);
                }
            }

            user.UpdateName(request.Name);
            user.UpdateEmail(email);

            await _userRepository.UpdateAsync(user, cancellationToken);

            return MapToUserResponse(user);
        }

        public async Task<UserResponse> UpdatePhotoAsync(Guid id, UpdateUserPhotoRequest request, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty)
            {
                throw new DomainException("O identificador do usuário é obrigatório.");
            }

            if (request is null)
            {
                throw new DomainException( "Os dados da foto são obrigatórios.");
            }

            var user = await _userRepository.GetByIdAsync(id, cancellationToken) ?? throw new UserNotFoundException(id);

            user.UpdatePhoto(request.PhotoBase64, request.PhotoContentType);

            await _userRepository.UpdateAsync(user, cancellationToken);

            return MapToUserResponse(user);
        }

        public async Task<UserResponse> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = await GetUserAsync(id, cancellationToken);

            user.Activate();

            await _userRepository.UpdateAsync(user, cancellationToken);

            return MapToUserResponse(user);
        }

        public async Task<UserResponse> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = await GetUserAsync(id, cancellationToken);

            user.Deactivate();

            await _userRepository.UpdateAsync(user, cancellationToken);

            return MapToUserResponse(user);
        }

        public async Task<PagedResponse<UserResponse>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            if (page <= 0)
            {
                throw new DomainException("A página deve ser maior que zero.");
            }

            if (pageSize <= 0 || pageSize > 100)
            {
                throw new DomainException("O tamanho da página deve estar entre 1 e 100.");
            }

            var users = await _userRepository.GetPagedAsync(page, pageSize, cancellationToken);
            var totalItems = await _userRepository.CountAsync(cancellationToken);

            return new PagedResponse<UserResponse>
            {
                Items = users.Select(MapToUserResponse).ToList().AsReadOnly(),
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };
        }

        private async Task<User> GetUserAsync(Guid id, CancellationToken cancellationToken)
        {
            if (id == Guid.Empty)
            {
                throw new DomainException("O identificador do usuário é obrigatório.");
            }

            var user = await _userRepository.GetByIdAsync(id, cancellationToken);

            return user is null ? throw new UserNotFoundException(id) : user;
        }

        private static void ValidateRequest(RegisterUserRequest request)
        {
            if (request is null)
                throw new DomainException("Os dados do usuário são obrigatórios.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new DomainException("O nome do usuário é obrigatório.");

            if (string.IsNullOrWhiteSpace(request.Email))
                throw new DomainException("O e-mail é obrigatório.");

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new DomainException("A senha é obrigatória.");

            if (string.IsNullOrWhiteSpace(request.ConfirmPassword))
                throw new DomainException("A confirmação da senha é obrigatória.");

            ValidatePhoto(request.PhotoBase64, request.PhotoContentType);
        }

        private static void ValidatePassword(string password, string confirmPassword)
        {
            if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
            {
                throw new DomainException("A senha e a confirmação da senha não são iguais.");
            }

            if (password.Length < MinimumPasswordLength)
            {
                throw new DomainException($"A senha deve possuir no mínimo " + $"{MinimumPasswordLength} caracteres.");
            }

            if (password.Length > MaximumPasswordLength)
            {
                throw new DomainException($"A senha deve possuir no máximo " + $"{MaximumPasswordLength} caracteres.");
            }

            if (!password.Any(char.IsUpper))
            {
                throw new DomainException("A senha deve possuir pelo menos uma letra maiúscula.");
            }

            if (!password.Any(char.IsLower))
            {
                throw new DomainException("A senha deve possuir pelo menos uma letra minúscula.");
            }

            if (!password.Any(char.IsDigit))
            {
                throw new DomainException("A senha deve possuir pelo menos um número.");
            }

            if (!password.Any(character =>
                    !char.IsLetterOrDigit(character)))
            {
                throw new DomainException("A senha deve possuir pelo menos um caractere especial.");
            }
        }

        private static void ValidatePhoto(string? photoBase64, string? photoContentType)
        {
            var hasPhoto = !string.IsNullOrWhiteSpace(photoBase64);
            var hasContentType =
                !string.IsNullOrWhiteSpace(photoContentType);

            if (hasPhoto && !hasContentType)
            {
                throw new DomainException("O tipo do arquivo da foto é obrigatório.");
            }

            if (!hasPhoto && hasContentType)
            {
                throw new DomainException("A foto deve ser informada junto com o tipo do arquivo.");
            }
        }

        private static RegisterUserResponse MapToResponse(User user)
        {
            return new RegisterUserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email.Value,
                PhotoBase64 = user.PhotoBase64,
                PhotoContentType = user.PhotoContentType,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        private static UserResponse MapToUserResponse(User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email.Value,
                PhotoBase64 = user.PhotoBase64,
                PhotoContentType = user.PhotoContentType,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
    }
}
