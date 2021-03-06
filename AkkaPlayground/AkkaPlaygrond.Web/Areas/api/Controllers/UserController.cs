﻿using AkkaPlayground.Core.Data;
using System;
using System.Linq;
using System.Web.Mvc;
using MongoDB.Driver;
using AkkaPlayground.Projections;
using AkkaPlaygrond.Web.Models;
using AkkaPlaygrond.Web.Actors;
using Akka.Actor;
using System.Collections.Generic;
using AkkaPlayground.Messages.Commands;

namespace AkkaPlaygrond.Web.Areas.api.Controllers
{
    public class UserController : Controller
    {
        private MongoContext _mongoContext;

        public UserController()
        {
            _mongoContext = new MongoContext();
        }

        [HttpPost]
        [AllowAnonymous]
        public Guid Register(RegisterModel model)
        {
            Guid userId = Guid.NewGuid();
            model.UserId = userId;
            SystemActors.SignalRActor.Tell(model, ActorRefs.Nobody);
            return userId;
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult Login(string login)
        {
            var user = _mongoContext.Users().AsQueryable<UserDto>().FirstOrDefault(x => x.Login == login);
            if (user == null)
            {
                throw new ArgumentNullException();
            }
            return Json(user, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AddToContactList(Guid userId, Guid targetUserId)
        {
            SubscribeToUserCommand command = new SubscribeToUserCommand(userId, targetUserId);
            SystemActors.SignalRActor.Tell(command);

            return Json(true);
        }

        [HttpPost]
        public JsonResult MarkChatMessagesRead(Guid userId, Guid chatId)
        {
            MarkChatMessagesReadCommand command = new MarkChatMessagesReadCommand(userId, chatId);
            SystemActors.SignalRActor.Tell(command);

            return Json(true);
        }


        [HttpPost]
        public JsonResult Search(Guid currentUserId, string searchString)
        {
            List<UserDto> userProjections =
                (from x in _mongoContext.Users().AsQueryable()
                 where x.Login.Contains(searchString) ||
                    (x.UserName != null && x.UserName.Contains(searchString))
                 select x).Take(20).ToList();

            IEnumerable<Guid> foundUserIds = userProjections.Select(x => x.Id).ToList();
            var usersAlreadyInContacts =
                (from c in _mongoContext.UserContacts().AsQueryable()
                where c.UserId == currentUserId && foundUserIds.Contains(c.ContactUserId)
                select c).ToList();

            List<UserSearchModel> result = userProjections.Select(item =>
                new UserSearchModel()
                {
                    Id = item.Id,
                    Login = item.Login,
                    UserName = item.UserName,
                    IsAlreadyAdded = usersAlreadyInContacts.Any(x => x.ContactUserId == item.Id),
                    IsCurrentUser = item.Id == currentUserId
                }
            ).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetUserContacts(Guid userId)
        {
            List<UserContactsDto> userContacts =
                (from c in _mongoContext.UserContacts().AsQueryable()
                 where c.UserId == userId
                 select c).ToList();

            List<UserContactModel> result = userContacts.Select(item =>
                new UserContactModel()
                {
                    Id = item.ContactUserId,
                    Login = item.ContactLogin,
                    UserName = item.ContactUserName
                }).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
