﻿using Financial.Chat.Domain.Core.CommandHandlers;
using Financial.Chat.Domain.Core.Commands;
using Financial.Chat.Domain.Shared.Entity;
using Financial.Chat.Domain.Shared.Handler;
using Financial.Chat.Domain.Shared.Notifications;
using Financial.Chat.Tests.ContextDb;
using Financial.Chat.Web.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading;
using System.Threading.Tasks;

namespace Financial.Chat.Tests.Controllers
{
    [TestClass]
    public class LoginControllerTest : FinancialChatDbContextFixure
    {
        private Mock<IMediatorHandler> _mockMediator;
        private DomainNotificationHandler _domainNotificationHandler;

        [TestInitialize]
        public void InitTests()
        {
            _mockMediator = new Mock<IMediatorHandler>();
            _domainNotificationHandler = new DomainNotificationHandler();
            _mockMediator.Setup(x => x.RaiseEvent(It.IsAny<DomainNotification>())).Callback<DomainNotification>((x) =>
            {
                _domainNotificationHandler.Handle(x, CancellationToken.None);
            });
        }

        [TestMethod]
        public async Task Should_not_get_authenticated_return_unathourized()
        {
            var obj = new AuthenticateUserCommand { Email = "test@test.com", Password = "123" };
            _mockMediator.Setup(x => x.SendCommandResult(It.IsAny<GenericCommandResult<bool>>())).Returns(Task.FromResult(false));
            var result = await new LoginController(_domainNotificationHandler, _mockMediator.Object).LoginAsync(obj) as UnauthorizedResult;

            Assert.IsTrue(result.StatusCode == 401);
        }

        [TestMethod]
        public async Task Should_get_authenticated_token()
        {
            string tokenExpected = "asASDNdBNASbdaskjdbabksdavbsklDAPsdh";
            var obj = new AuthenticateUserCommand { Email = "yago.oliveira.ce@live.com", Password = "123456" };
            _mockMediator.Setup(x => x.SendCommandResult(It.IsAny<GenericCommandResult<TokenJWT>>())).Returns(Task.FromResult(new TokenJWT
            (
                true,
                "asASDNdBNASbdaskjdbabksdavbsklDAPsdh"
            )));
            var result = (await new LoginController(_domainNotificationHandler, _mockMediator.Object).LoginAsync(obj) as OkObjectResult).Value as ApiOkReturn;
            var token = result.data as TokenJWT;

            Assert.IsTrue(result.success);
            Assert.AreEqual(tokenExpected, token.Token);
        }
    }
}
