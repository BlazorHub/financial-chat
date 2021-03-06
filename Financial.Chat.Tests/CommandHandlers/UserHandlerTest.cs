﻿using AutoMapper;
using Financial.Chat.Application.AutoMapper;
using Financial.Chat.Domain.Core.CommandHandlers;
using Financial.Chat.Domain.Core.Commands;
using Financial.Chat.Domain.Core.Commands.Message;
using Financial.Chat.Domain.Core.Entity;
using Financial.Chat.Domain.Core.Interfaces;
using Financial.Chat.Domain.Shared.Handler;
using Financial.Chat.Domain.Shared.Notifications;
using Financial.Chat.Infra.Data;
using Financial.Chat.Infra.Data.Repositories;
using Financial.Chat.Tests.ContextDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Financial.Chat.Tests.CommandHandlers
{
    [TestClass]
    public class UserHandlerTest : FinancialChatDbContextFixure
    {
        private IUnitOfWork _unitOfWork;
        private Mock<IMediatorHandler> _mockMediator;
        private IUserRepository _userRepository;
        private DomainNotificationHandler _domainNotificationHandler;
        private IMapper _mapper;
        private UserHandler handler;

        [TestInitialize]
        public void InitTests()
        {
            db = GetDbInstance();
            _unitOfWork = new UnitOfWork(db);
            _userRepository = new UserRepository(db);
            _mockMediator = new Mock<IMediatorHandler>();
            _domainNotificationHandler = new DomainNotificationHandler();
            _mockMediator.Setup(x => x.RaiseEvent(It.IsAny<DomainNotification>())).Callback<DomainNotification>((x) =>
            {
                _domainNotificationHandler.Handle(x, CancellationToken.None);
            });

            _mapper = AutoMapperConfig.RegisterMappings().CreateMapper();

            handler = new UserHandler(_unitOfWork, _userRepository, _mockMediator.Object, _mapper);
        }

        [TestMethod]
        public async Task Should_not_register_user_name_is_required()
        {
            var user = new UserAddCommand
            {
                Email = "yago.oliveira.ce@live.com",
                Password = "123456",
                SecondPassword = "123456"
            };

            var result = await handler.Handle(user, CancellationToken.None);

            Assert.IsFalse(result);
            Assert.IsTrue(_domainNotificationHandler.HasNotifications());
        }
        [TestMethod]
        public async Task Should_not_register_email_is_invalid()
        {
            string expectedMessageError = "A valid email address is required.";
            var user = new UserAddCommand
            {
                Email = "yago.oliveira.celive.com",
                Password = "123456",
                SecondPassword = "123456",
                Name =  "Yago"
            };

            var result = await handler.Handle(user, CancellationToken.None);

            Assert.IsFalse(result);
            Assert.IsTrue(_domainNotificationHandler.GetNotifications().Any(x => x.Value == expectedMessageError));
        }

        [TestMethod]
        public async Task Should_not_register_password_are_not_equal()
        {
            string expectedMessageError = "The passwords are not equals";
            var user = new UserAddCommand
            {
                Email = "yago.oliveira.ce@live.com",
                Password = "123456",
                SecondPassword = "123465",
                Name = "Yago"
            };

            var result = await handler.Handle(user, CancellationToken.None);

            Assert.IsFalse(result);
            Assert.IsTrue(_domainNotificationHandler.GetNotifications().Any(x => x.Value == expectedMessageError));
        }

        [TestMethod]
        public async Task Should_not_register_password_have_less_than_six()
        {
            string expectedMessageError = "The password must have minimum 6 characters";
            var user = new UserAddCommand
            {
                Email = "yago.oliveira.ce@live.com",
                Password = "12345",
                SecondPassword = "12345",
                Name = "Yago"
            };

            var result = await handler.Handle(user, CancellationToken.None);

            Assert.IsFalse(result);
            Assert.IsTrue(_domainNotificationHandler.GetNotifications().Any(x => x.Value == expectedMessageError));
        }
        [TestMethod]
        public async Task Should_register_user_valid()
        {
            var user = new UserAddCommand
            {
                Email = "yago.oliveira.ce@live.com",
                Password = "123456",
                SecondPassword = "123456",
                Name = "Yago"
            };

            var result = await handler.Handle(user, CancellationToken.None);

            Assert.IsTrue(result);
            Assert.IsFalse(_domainNotificationHandler.HasNotifications());
        }

        [TestMethod]
        public async Task Should_not_register_message_field_message_is_required()
        {
            string expectedMessageError = "The message is required.";
            var message = new MessageAddCommand()
            {
                Sender = "yago.oliveira.ce@gmail.com"
            };

            var result = await handler.Handle(message, CancellationToken.None);

            Assert.IsFalse(result);
            Assert.IsTrue(_domainNotificationHandler.GetNotifications().Any(x => x.Value == expectedMessageError));
        }

        [TestMethod]
        public async Task Should_not_register_message_field_sender_is_required()
        {
            string expectedMessageError = "The sender is required.";
            var message = new MessageAddCommand()
            {
                Message = "Hello world"
            };

            var result = await handler.Handle(message, CancellationToken.None);

            Assert.IsFalse(result);
            Assert.IsTrue(_domainNotificationHandler.GetNotifications().Any(x => x.Value == expectedMessageError));
        }

        [TestMethod]
        public async Task Should_register_messsage()
        {
            var message = new MessageAddCommand()
            {
                Message = "Hello world",
                Sender = "yago.oliveira.ce@gmail.com"
            };

            var result = await handler.Handle(message, CancellationToken.None);

            Assert.IsTrue(result);
            Assert.IsFalse(_domainNotificationHandler.HasNotifications());
        }
    }
}
