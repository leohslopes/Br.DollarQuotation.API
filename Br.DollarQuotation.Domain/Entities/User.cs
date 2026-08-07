using Br.DollarQuotation.Domain.Common;
using Br.DollarQuotation.Domain.Exceptions;
using Br.DollarQuotation.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Br.DollarQuotation.Domain.Entities
{
    public class User : Entity
    {
        private const int MinimumNameLength = 3;
        private const int MaximumNameLength = 150;

        public string Name { get; private set; } = string.Empty;

        public Email Email { get; private set; } = null!;

        public string PasswordHash { get; private set; } = string.Empty;

        public string? PhotoBase64 { get; private set; }

        public string? PhotoContentType { get; private set; }

        public bool IsActive { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime? UpdatedAt { get; private set; }

        protected User()
        {

        }

        public User(
            string name,
            Email email,
            string passwordHash,
            string? photoBase64 = null,
            string? photoContentType = null)
        {
            SetName(name);
            SetEmail(email);
            SetPasswordHash(passwordHash);

            CreatedAt = DateTime.UtcNow;
            IsActive = true;

            if (!string.IsNullOrWhiteSpace(photoBase64))
            {
                UpdatePhoto(photoBase64, photoContentType);
            }
        }

        public void UpdateName(string name)
        {
            SetName(name);
            SetUpdatedAt();
        }

        public void UpdateEmail(Email email)
        {
            SetEmail(email);
            SetUpdatedAt();
        }

        public void UpdatePassword(string passwordHash)
        {
            SetPasswordHash(passwordHash);
            SetUpdatedAt();
        }

        public void UpdatePhoto(string photoBase64, string photoContentType)
        {
            if (string.IsNullOrWhiteSpace(photoBase64))
            {
                throw new DomainException("A foto é obrigatória.");
            }

            if (string.IsNullOrWhiteSpace(photoContentType))
            {
                throw new DomainException("O tipo da imagem é obrigatório.");
            }

            var allowedContentTypes = new[]
            {
        "image/png",
        "image/jpeg",
        "image/webp"
            };

            if (!allowedContentTypes.Contains(photoContentType,StringComparer.OrdinalIgnoreCase))
            {
                throw new DomainException("Formato de imagem não permitido. Utilize PNG, JPEG ou WEBP.");
            }

            var normalizedBase64 = NormalizeBase64(photoBase64);

            byte[] imageBytes;

            try
            {
                imageBytes = Convert.FromBase64String(normalizedBase64);
            }
            catch (FormatException)
            {
                throw new DomainException( "A imagem informada não possui um Base64 válido.");
            }

            const int maxPhotoSizeInBytes = 2 * 1024 * 1024;

            if (imageBytes.Length > maxPhotoSizeInBytes)
            {
                throw new DomainException("A foto deve possuir no máximo 2 MB.");
            }

            PhotoBase64 = normalizedBase64;
            PhotoContentType = photoContentType;
            UpdatedAt = DateTime.UtcNow;
        }

        
        public void RemovePhoto()
        {
            PhotoBase64 = null;
            PhotoContentType = null;

            SetUpdatedAt();
        }

        public void Activate()
        {
            if (IsActive)
                return;

            IsActive = true;
            SetUpdatedAt();
        }

        public void Deactivate()
        {
            if (!IsActive)
                return;

            IsActive = false;
            SetUpdatedAt();
        }

        private void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("O nome é obrigatório.");

            var normalizedName = name.Trim();

            if (normalizedName.Length < MinimumNameLength)
            {
                throw new DomainException($"O nome deve possuir no mínimo " + $"{MinimumNameLength} caracteres.");
            }

            if (normalizedName.Length > MaximumNameLength)
            {
                throw new DomainException($"O nome deve possuir no máximo " + $"{MaximumNameLength} caracteres.");
            }

            Name = normalizedName;
        }

        private void SetEmail(Email email)
        {
            Email = email ?? throw new DomainException("O e-mail é obrigatório.");
        }

        private void SetPasswordHash(string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
            {
                throw new DomainException("O hash da senha é obrigatório.");
            }

            PasswordHash = passwordHash;
        }

        private void SetUpdatedAt()
        {
            UpdatedAt = DateTime.UtcNow;
        }

        private static string NormalizeBase64(string photoBase64)
        {
            var normalizedBase64 = photoBase64.Trim();
            var commaIndex = normalizedBase64.IndexOf(',');

            if (normalizedBase64.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && commaIndex >= 0)
            {
                normalizedBase64 = normalizedBase64[(commaIndex + 1)..];
            }

            try
            {
                Convert.FromBase64String(normalizedBase64);
            }
            catch (FormatException)
            {
                throw new DomainException("O conteúdo da foto não possui um Base64 válido.");
            }

            return normalizedBase64;
        }
    }
}
