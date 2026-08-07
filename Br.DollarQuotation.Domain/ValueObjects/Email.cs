using Br.DollarQuotation.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Br.DollarQuotation.Domain.ValueObjects
{
    public sealed class Email : IEquatable<Email>
    {
        private const int MaxLength = 200;

        private static readonly Regex EmailRegex = new( @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public string Value { get; }

        private Email(string value)
        {
            Value = value;
        }

        public static Email Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("O e-mail é obrigatório.");

            var normalizedEmail = value.Trim().ToLowerInvariant();

            if (normalizedEmail.Length > MaxLength)
            {
                throw new DomainException($"O e-mail deve possuir no máximo {MaxLength} caracteres.");
            }

            if (!EmailRegex.IsMatch(normalizedEmail))
                throw new DomainException("O e-mail informado é inválido.");

            return new Email(normalizedEmail);
        }

        public bool Equals(Email? other)
        {
            if (other is null)
                return false;

            return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj)
        {
            return obj is Email other && Equals(other);
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(Value);
        }

        public override string ToString()
        {
            return Value;
        }

        public static implicit operator string(Email email)
        {
            return email.Value;
        }

        public static explicit operator Email(string value)
        {
            return Create(value);
        }

        public static bool operator == (Email? left, Email? right)
        {
            return Equals(left, right);
        }

        public static bool operator != (Email? left, Email? right)
        {
            return !Equals(left, right);
        }
    }
}
