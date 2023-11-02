using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace AspNetIdentity.Extensions;

public static class Security
{
    public static string CreateSalt(int size = 16)
    {
        // Generate a cryptographic random number.
        RNGCryptoServiceProvider rng = new();
        byte[] buff = new byte[size];
        rng.GetBytes(buff);
        return Convert.ToBase64String(buff);
    }

    public static string EncodePassword(string pass, string salt)
    {
        byte[] bIn = Encoding.Unicode.GetBytes(pass);
        byte[] bSalt = Convert.FromBase64String(salt);
        byte[] bRet;

        HashAlgorithm hm = new SHA1Managed();
        if (hm is KeyedHashAlgorithm kha)
        {
            if (kha.Key.Length == bSalt.Length)
            {
                kha.Key = bSalt;
            }
            else if (kha.Key.Length < bSalt.Length)
            {
                byte[] bKey = new byte[kha.Key.Length];
                Buffer.BlockCopy(bSalt, 0, bKey, 0, bKey.Length);
                kha.Key = bKey;
            }
            else
            {
                byte[] bKey = new byte[kha.Key.Length];
                for (int iter = 0; iter < bKey.Length;)
                {
                    int len = Math.Min(bSalt.Length, bKey.Length - iter);
                    Buffer.BlockCopy(bSalt, 0, bKey, iter, len);
                    iter += len;
                }
                kha.Key = bKey;
            }
            bRet = kha.ComputeHash(bIn);
        }
        else
        {
            byte[] bAll = new byte[bSalt.Length + bIn.Length];
            Buffer.BlockCopy(bSalt, 0, bAll, 0, bSalt.Length);
            Buffer.BlockCopy(bIn, 0, bAll, bSalt.Length, bIn.Length);
            bRet = hm.ComputeHash(bAll);
        }

        return Convert.ToBase64String(bRet);
    }

    public static string CreatePasswordHash(byte version, string salt, string hash)
    {
        byte[] bSalt = Convert.FromBase64String(salt);
        byte[] bHash = Convert.FromBase64String(hash);

        byte[] bAll = new byte[1 + bSalt.Length + bHash.Length];
        bAll[0] = version;

        Buffer.BlockCopy(bSalt, 0, bAll, 1, bSalt.Length);
        Buffer.BlockCopy(bHash, 0, bAll, 1 + bSalt.Length, bHash.Length);

        var result = Convert.ToBase64String(bAll);
        return result;
    }

    public static bool VerifyHashedPasswordV2(byte[] hashedPassword, string password)
    {
        const int versionSize = 1;
        const int saltSize = 16;
        int hashSize = (hashedPassword.Length - versionSize - saltSize);

        // We know ahead of time the exact length of a valid hashed password payload.
        if (hashSize < 20)
        {
            return false; // bad size
        }

        byte[] salt = new byte[saltSize];
        Buffer.BlockCopy(hashedPassword, versionSize, salt, 0, saltSize);

        byte[] hash = new byte[hashSize];
        Buffer.BlockCopy(hashedPassword, versionSize + saltSize, hash, 0, hashSize);

        var passwordHash = Convert.FromBase64String(EncodePassword(password, Convert.ToBase64String(salt)));

        // Hash the incoming password and verify it
        return ByteArraysEqual(passwordHash, hash);
    }

    // Compares two byte arrays for equality.
    // The method is specifically written so that the loop is not optimized.
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static bool ByteArraysEqual(byte[] a, byte[] b)
    {
        if (a == null && b == null)
        {
            return true;
        }
        if (a == null || b == null || a.Length != b.Length)
        {
            return false;
        }
        var areSame = true;
        for (var i = 0; i < a.Length; i++)
        {
            areSame &= (a[i] == b[i]);
        }
        return areSame;
    }
}