﻿using System;
using System.Net;
using System.Runtime.Serialization;
using ICAds.Data.DTO;
using ICAds.Data.Models;
using ICAds.Security;
using Microsoft.EntityFrameworkCore;

namespace ICAds.Data.Repositories
{
    public class UserRepository
    {
        public static async Task<UserModel> CreateUser(UserDTO user, AppDataContext db)
        {

            var hashedPassword = SecurityManager.HashPassword(user.Password);
            UserModel newUser = new UserModel();

            newUser.Id = Guid.NewGuid().ToString();
            newUser.Firstname = user.Firstname;
            newUser.Lastname = user.Lastname;
            newUser.Email = user.Email;
            newUser.PasswordHash = hashedPassword.Hash;
            newUser.PasswordSalt = hashedPassword.Salt;
            
            db.Users.Add(newUser);
            await db.SaveChangesAsync();

            return newUser;

        }

        public static async Task<string> LoginUser(LoginDTO credentials)
        {
            // Check if user exists
            var user = await GetUser(credentials.Email);
            if (user == null) return null;

            // Check if passwords match
            if (!SecurityManager.CheckPassword(credentials.Password, user.PasswordHash, user.PasswordSalt)) return null;
         
            // Create Token
            var token = SecurityManager.CreateToken(user.Id, user.OrganizationId, user.Email);

            // Return Token
            return token;
        }

        public static async Task<UserModel> GetUser(string email)
        {
            using (var db = new AppDataContext())
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
                return user;
            }
        }


        public static async Task<UserModel> GetUserWithContext(string email, AppDataContext db)
        {
                // Db context sent as parameter because connection is already established in other method
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            return user;
        }


        public static List<UserModel> GetAllUsers()
        {
            using (var db = new AppDataContext())
            {
                List<UserModel> users = db.Users.ToList();
                return users;
            }
        }
    }

  
}

