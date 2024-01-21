using System.Security.Cryptography;

using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Users.Library.Securities;

// REF: Copy and modified from "https://github.com/dotnet/aspnetcore/blob/main/src/Identity/Extensions.Core/src/PasswordHasher.cs".
internal class PasswordHasher
{
    private readonly int _iterCount = 100_000;

    private readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

    private readonly int _saltSize = 128 / 8;

    private readonly int _numBytesRequested = 256 / 8;

    private readonly KeyDerivationPrf _prf = KeyDerivationPrf.HMACSHA512;

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentNullException(nameof(password));
        }

        byte[] salt = new byte[_saltSize];
        _rng.GetBytes(salt);
        byte[] subkey = KeyDerivation.Pbkdf2(password, salt, _prf, _iterCount, _numBytesRequested);

        byte[] outputBytes = new byte[13 + salt.Length + subkey.Length];
        outputBytes[0] = 0x01; // Format marker.
        WriteNetworkByteOrder(outputBytes, 1, (uint)_prf);
        WriteNetworkByteOrder(outputBytes, 5, (uint)_iterCount);
        WriteNetworkByteOrder(outputBytes, 9, (uint)_saltSize);
        Buffer.BlockCopy(salt, 0, outputBytes, 13, salt.Length);
        Buffer.BlockCopy(subkey, 0, outputBytes, 13 + _saltSize, subkey.Length);

        return Convert.ToBase64String(outputBytes);
    }

    public bool VerifyHashedPassword(string hashedPassword, string providedPassword)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword))
        {
            throw new ArgumentNullException(nameof(hashedPassword));
        }

        if (string.IsNullOrWhiteSpace(providedPassword))
        {
            throw new ArgumentNullException(nameof(providedPassword));
        }

        byte[] decodedHashedPassword = Convert.FromBase64String(hashedPassword);

        try
        {
            // Read header information.
            var prf = (KeyDerivationPrf)ReadNetworkByteOrder(decodedHashedPassword, 1);
            int iterCount = (int)ReadNetworkByteOrder(decodedHashedPassword, 5);
            int saltLength = (int)ReadNetworkByteOrder(decodedHashedPassword, 9);

            // Read the salt: must be >= 128 bits.
            if (saltLength < 128 / 8)
            {
                return false;
            }

            byte[] salt = new byte[saltLength];
            Buffer.BlockCopy(decodedHashedPassword, 13, salt, 0, salt.Length);

            // Read the subkey (the rest of the payload): must be >= 128 bits.
            int subkeyLength = decodedHashedPassword.Length - 13 - salt.Length;

            if (subkeyLength < 128 / 8)
            {
                return false;
            }

            byte[] expectedSubkey = new byte[subkeyLength];
            Buffer.BlockCopy(decodedHashedPassword, 13 + salt.Length, expectedSubkey, 0, expectedSubkey.Length);

            // Hash the incoming password and verify it.
            byte[] actualSubkey = KeyDerivation.Pbkdf2(providedPassword, salt, prf, iterCount, subkeyLength);
            return CryptographicOperations.FixedTimeEquals(actualSubkey, expectedSubkey);
        }
        catch
        {
            // This should never occur except in the case of a malformed payload, where we might go
            // off the end of the array. Regardless, a malformed payload implies verification failed.
            return false;
        }
    }

    private static uint ReadNetworkByteOrder(byte[] buffer, int offset)
    {
        return ((uint)buffer[offset + 0] << 24)
            | ((uint)buffer[offset + 1] << 16)
            | ((uint)buffer[offset + 2] << 8)
            | buffer[offset + 3];
    }

    private static void WriteNetworkByteOrder(byte[] buffer, int offset, uint value)
    {
        buffer[offset + 0] = (byte)(value >> 24);
        buffer[offset + 1] = (byte)(value >> 16);
        buffer[offset + 2] = (byte)(value >> 8);
        buffer[offset + 3] = (byte)(value >> 0);
    }
}
