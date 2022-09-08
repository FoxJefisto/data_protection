﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Shapes;

namespace TestApp.Model
{
    public class UserManager
    {
        private Database DB { get; set; }
        public User CurrentUser { get; set; } = null;

        public string pathDatabase;

        public bool isFirstExecute;

        public UserManager(string pathDatabase)
        {
            this.pathDatabase = pathDatabase;
            DB = new Database(pathDatabase);
            isFirstExecute = !DB.LoadFromDB();
        }

        public (bool state, string message) LogIn(string login, string password)
        {
            bool state = true;
            string message = "Операция выполнена успешно";
            login = login.ToLower();
            if (DB.Users.Any(x => string.Equals(x.Login, login, StringComparison.OrdinalIgnoreCase)))
            {
                CurrentUser = DB.Users.Where(x => string.Equals(x.Login, login, StringComparison.OrdinalIgnoreCase) && x.Password == password).FirstOrDefault();
                if (CurrentUser == null)
                {
                    state = false;
                    message = "Был введен неверный пароль";
                }
            }
            else
            {
                state = false;
                message = "Пользователь с данным логином не найден";
            }
            return (state, message);
        }

        public void LogOut()
        {
            CurrentUser = null;
        }

        public (bool state, string message) SignUp(string login, string password, bool rootAccess = false, bool ignorePassword = false)
        {
            bool state = true;
            string message = "Операция выполнена успешно";
            login = login.ToLower();
            var matchPassword = Regex.Match(password, @"[0-9]+[\.,:\-""?!;\(\)]+[0-9]+([\.,:\-""?!;\(\)]+[0-9]+)*");
            if (matchPassword.Success || ignorePassword)
            {
                if (!DB.Users.Any(x => string.Equals(x.Login, login, StringComparison.OrdinalIgnoreCase)))
                {
                    User user;
                    if (rootAccess)
                    {
                        user = new User(login, password, true);
                        isFirstExecute = false;
                    }
                    else
                        user = new User(login, password, false);
                    DB.Users.Add(user);
                }
                else
                {
                    state = false;
                    message = "Пользователь с таким логином уже существует";
                }
            }
            else
            {
                state = false;
                message = "Пароль не соответствует требованиям";
            }
            return (state, message);
        }

        public (bool state, string message) RemoveUser(string login)
        {
            bool state = true;
            string message = "Операция выполнена успешно";
            User user = DB.Users.Where(x => string.Equals(x.Login, login, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            state = user != null;
            if(state)
            {
                DB.Users.Remove(user);
            }
            else
            {
                message = "Пользователь с таким именем не зарегистрирован";
            }
            return (state, message);
        }

        public (bool state, string message) ChangeCurrentUserPassword(string oldPassword, string newPassword)
        {
            bool state = true;
            string message = "Операция выполнена успешно";
            if (oldPassword == CurrentUser.Password)
            {
                var matchPassword = Regex.Match(newPassword, @"[0-9]+[\.,:\-""?!;\(\)]+[0-9]+([\.,:\-""?!;\(\)]+[0-9]+)*");
                if (!CurrentUser.HasConstraint || matchPassword.Success)
                {
                    CurrentUser.Password = newPassword;
                    DB.SaveDB();
                }
                else
                {
                    state = false;
                    message = "Новый пароль не соответствует требованиям";
                }
            }
            else
            {
                state = false;
                message = "Старый пароль введен неверно";
            }
            return (state, message);
        }

        public ObservableCollection<User> GetAllUsers()
        {
            if (!CurrentUser.RootAccess)
                throw new Exception("У данного пользователя нет таких прав");
            return DB.Users;
        }

        public (bool state, string message) RegisterLoginForUser(string login)
        {
            if (!CurrentUser.RootAccess)
                throw new Exception("У данного пользователя нет таких прав");
            bool state = true;
            string message = "Операция выполнена успешно";
            if (!DB.Users.Any(x => string.Equals(x.Login, login, StringComparison.OrdinalIgnoreCase)))
            {
                DB.Users.Add(new User(login, "", false));
            }
            else
            {
                state = false;
                message = "Пользователь с таким логином уже существует";
            }
            return (state, message);
        }

        public void SaveChanges()
        {
            DB.SaveDB();
        }

        //public (bool state, string message) ChangeBanStatus(string login, bool status)
        //{
        //    if (!CurrentUser.RootAccess)
        //        throw new Exception("У данного пользователя нет таких прав");
        //    bool state = true;
        //    string message = "Операция выполнена успешно";
        //    User user = DB.Users.Where(x => string.Equals(x.Login, login, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        //    if (user != null)
        //    {
        //        user.IsBanned = status;
        //        DB.SaveDB();
        //    }
        //    else
        //    {
        //        state = false;
        //        message = "Пользователь с таким логином не найден";
        //    }
        //    return (state, message);
        //}

        //public (bool state, string message) ChangeConstraint(string login, bool status)
        //{
        //    if (!CurrentUser.RootAccess)
        //        throw new Exception("У данного пользователя нет таких прав");
        //    bool state = true;
        //    string message = "Операция выполнена успешно";
        //    User user = DB.Users.Where(x => string.Equals(x.Login, login, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        //    if (user != null)
        //    {
        //        user.HasConstraint = status;
        //        DB.SaveDB();
        //    }
        //    else
        //    {
        //        state = false;
        //        message = "Пользователь с таким логином не найден";
        //    }
        //    return (state, message);
        //}
    }
}